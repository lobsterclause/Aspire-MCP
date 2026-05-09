using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI;

/// <summary>
/// Service responsible for managing authentication tokens for the Aspire API.
/// Handles token acquisition, caching, refreshing, and error recovery.
/// </summary>
public sealed class TokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenService> _logger;
    private readonly AspireApiOptions _options;
    private readonly string _baseUrl;
    
    // Cache keys
    private const string ACCESS_TOKEN_CACHE_KEY = "Aspire:AccessToken";
    private const string REFRESH_TOKEN_CACHE_KEY = "Aspire:RefreshToken";
    
    // Lock object to prevent concurrent token refresh operations
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    // Status fields exposed via GetStatus(). Updated whenever tokens are cached
    // or invalidated. Reading them races with writes — that's fine for an
    // operator-facing health panel, where eventual consistency is plenty.
    private DateTime? _lastAcquiredAtUtc;
    private DateTime? _expiresAtUtc;
    private string? _lastError;
    private DateTime? _lastErrorAtUtc;

    /// <summary>
    /// Initializes a new instance of the TokenService class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
    /// <param name="options">Configuration options for the Aspire API</param>
    /// <param name="cache">Memory cache for storing tokens</param>
    /// <param name="logger">Logger for recording service activity</param>
    public TokenService(
        IHttpClientFactory httpClientFactory,
        IOptions<AspireApiOptions> options,
        IMemoryCache cache,
        ILogger<TokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
        
        // Get base URL from options, fallback to environment variable if not set
        _baseUrl = !string.IsNullOrEmpty(_options.BaseUrl)
            ? _options.BaseUrl
            : (Environment.GetEnvironmentVariable("ASPIRE__BASE_URL") ?? "https://cloud-api.youraspire.com");
    }

    /// <summary>
    /// Ensures a valid access token is available and returns it.
    /// If the token is expired or about to expire, it will be refreshed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A valid access token</returns>
    public async Task<string> EnsureValidAsync(CancellationToken cancellationToken = default)
    {
        // If using API key authentication, use that instead of token-based auth
        if (!_options.Auth.UseTokenAuth)
        {
            return GetApiKey();
        }

        // Try to get the cached access token
        if (_cache.TryGetValue(ACCESS_TOKEN_CACHE_KEY, out string cachedToken))
        {
            return cachedToken;
        }

        // No valid token in cache, acquire token with synchronization to prevent multiple simultaneous token requests
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // Double-check if another thread has already refreshed the token
            if (_cache.TryGetValue(ACCESS_TOKEN_CACHE_KEY, out string refreshedToken))
            {
                return refreshedToken;
            }
            
            // Try to use refresh token if available
            if (_cache.TryGetValue(REFRESH_TOKEN_CACHE_KEY, out string refreshToken))
            {
                try
                {
                    var newToken = await RefreshTokenAsync(refreshToken, cancellationToken);
                    if (newToken != null)
                    {
                        return newToken;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh token, will attempt to login again");
                }
            }
            
            // If refresh failed or no refresh token available, acquire a new token
            return await LoginAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new access token if successful, otherwise null</returns>
    private async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Refreshing access token");
        
        var client = _httpClientFactory.CreateClient();
        var refreshUrl = $"{_baseUrl.TrimEnd('/')}/Authorization/RefreshToken";
        
        try
        {
            var response = await client.PostAsJsonAsync(
                refreshUrl,
                new { refreshToken },
                cancellationToken);
                
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to refresh token: {StatusCode}", response.StatusCode);
                // Clear the cached refresh token as it's no longer valid
                _cache.Remove(REFRESH_TOKEN_CACHE_KEY);
                return null;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            
            // Cache the new tokens
            CacheTokens(tokenResponse);
            
            return tokenResponse.Access;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while refreshing token");
            return null;
        }
    }

    /// <summary>
    /// Performs a login to acquire new tokens.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new access token</returns>
    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Acquiring new access token");
        
        // If API key auth is configured and preferred, use that instead
        if (!_options.Auth.UseTokenAuth)
        {
            return GetApiKey();
        }

        // Get credentials from options with environment variable fallback
        var username = _options.Auth.Username ?? Environment.GetEnvironmentVariable("ASPIRE__USERNAME");
        var password = _options.Auth.Password ?? Environment.GetEnvironmentVariable("ASPIRE__PASSWORD");
        var companyKey = _options.Auth.CompanyKey ?? Environment.GetEnvironmentVariable("ASPIRE__COMPANYKEY");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(companyKey))
        {
            _logger.LogError("Aspire credentials not configured. Set credentials in appsettings.json or environment variables");
            throw new InvalidOperationException(
                "Aspire credentials not configured. Set AspireApi:Auth:Username, AspireApi:Auth:Password, and AspireApi:Auth:CompanyKey " +
                "in appsettings.json or set ASPIRE__USERNAME, ASPIRE__PASSWORD, and ASPIRE__COMPANYKEY environment variables."
            );
        }

        var client = _httpClientFactory.CreateClient();
        var loginUrl = $"{_baseUrl.TrimEnd('/')}/Authorization/Login";
        
        try
        {
            _logger.LogDebug("Sending login request to {LoginUrl}", loginUrl);
            
            var response = await client.PostAsJsonAsync(
                loginUrl,
                new { username, password, companyKey },
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to authenticate with Aspire API: {StatusCode}, {Error}",
                    response.StatusCode, errorContent);
                    
                throw new InvalidOperationException(
                    $"Failed to authenticate with Aspire API: {response.StatusCode}, {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            
            // Cache the tokens
            CacheTokens(tokenResponse);
            
            return tokenResponse.Access;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while logging in");
            throw new InvalidOperationException("Failed to connect to Aspire API", ex);
        }
    }

    /// <summary>
    /// Gets the API key from configuration or environment variables.
    /// </summary>
    /// <returns>The API key</returns>
    private string GetApiKey()
    {
        var apiKey = _options.ApiKey ?? Environment.GetEnvironmentVariable("ASPIRE__API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("Aspire API key not configured. Set ApiKey in appsettings.json or ASPIRE__API_KEY environment variable");
            throw new InvalidOperationException(
                "Aspire API key not configured. Set AspireApi:ApiKey in appsettings.json or ASPIRE__API_KEY environment variable."
            );
        }
        
        return apiKey;
    }

    /// <summary>
    /// Caches the access and refresh tokens with appropriate expiration.
    /// </summary>
    /// <param name="tokenResponse">The token response from the API</param>
    private void CacheTokens(TokenResponse tokenResponse)
    {
        // Calculate token expiration time, applying a buffer to refresh before it actually expires
        var refreshBeforeExpirationSeconds = _options.Auth.RefreshTokenBeforeExpirationSeconds;
        var expirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - refreshBeforeExpirationSeconds);
        
        _logger.LogDebug("Caching access token until {ExpirationTime}", expirationTime);
        
        // Cache the access token with expiration
        _cache.Set(ACCESS_TOKEN_CACHE_KEY, tokenResponse.Access, expirationTime);

        // Cache the refresh token with a longer expiration (usually refresh tokens last longer)
        // In a production system, you might want to store refresh tokens more securely
        _cache.Set(REFRESH_TOKEN_CACHE_KEY, tokenResponse.Refresh,
            DateTime.UtcNow.AddDays(30)); // Store refresh token for 30 days

        _lastAcquiredAtUtc = DateTime.UtcNow;
        _expiresAtUtc = expirationTime;
        _lastError = null;
        _lastErrorAtUtc = null;
    }

    /// <summary>
    /// Snapshot of the token cache for the admin health panel. Does not
    /// trigger a refresh; reflects only what's already in memory.
    /// </summary>
    public TokenStatus GetStatus()
    {
        var hasToken = _cache.TryGetValue(ACCESS_TOKEN_CACHE_KEY, out string? _);
        var hasRefresh = _cache.TryGetValue(REFRESH_TOKEN_CACHE_KEY, out string? _);
        return new TokenStatus(
            HasAccessToken: hasToken,
            HasRefreshToken: hasRefresh,
            AcquiredAtUtc: _lastAcquiredAtUtc,
            ExpiresAtUtc: _expiresAtUtc,
            UseTokenAuth: _options.Auth.UseTokenAuth,
            LastError: _lastError,
            LastErrorAtUtc: _lastErrorAtUtc);
    }

    internal void RecordError(string message)
    {
        _lastError = message;
        _lastErrorAtUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Invalidates the current token, forcing a new login on the next request.
    /// </summary>
    public void InvalidateToken()
    {
        _logger.LogInformation("Invalidating cached tokens");
        _cache.Remove(ACCESS_TOKEN_CACHE_KEY);
        _cache.Remove(REFRESH_TOKEN_CACHE_KEY);
    }
}

/// <summary>
/// Read-only snapshot of the token cache, used by the admin web UI's status panel.
/// </summary>
public sealed record TokenStatus(
    bool HasAccessToken,
    bool HasRefreshToken,
    DateTime? AcquiredAtUtc,
    DateTime? ExpiresAtUtc,
    bool UseTokenAuth,
    string? LastError,
    DateTime? LastErrorAtUtc);

/// <summary>
/// Response model for token authentication operations.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The access token used for API authorization.
    /// </summary>
    public string Access { get; set; }
    
    /// <summary>
    /// The refresh token used to obtain a new access token.
    /// </summary>
    public string Refresh { get; set; }
    
    /// <summary>
    /// The number of seconds until the access token expires.
    /// </summary>
    public int ExpiresIn { get; set; }
}