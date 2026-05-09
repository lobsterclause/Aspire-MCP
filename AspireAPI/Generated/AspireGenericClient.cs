using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.Generated
{
    /// <summary>
    /// Generic Aspire External REST API client used by every code-generated handler.
    /// Sends a request to {BaseUrl}{path} with the bearer token already issued by TokenService,
    /// returns the raw response body as a string. No per-resource DTOs.
    /// Mutating methods (POST/PUT/PATCH/DELETE) consult the production-write guard.
    /// </summary>
    public sealed class AspireGenericClient
    {
        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "POST", "PUT", "PATCH", "DELETE"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AspireApiOptions _options;
        private readonly ILogger<AspireGenericClient> _logger;

        public AspireGenericClient(
            IHttpClientFactory httpClientFactory,
            IOptions<AspireApiOptions> options,
            ILogger<AspireGenericClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Send a raw API request. Returns the response body verbatim (the Aspire API returns JSON).
        /// </summary>
        /// <param name="method">HTTP verb (GET/POST/PUT/PATCH/DELETE).</param>
        /// <param name="path">Path beginning with '/', e.g. "/Contacts" or "/OpportunityTags/123".</param>
        /// <param name="query">Optional query parameters (already-decoded values; will be URL-encoded).</param>
        /// <param name="body">Optional request body, serialized to JSON if non-null.</param>
        /// <param name="accessToken">Bearer token obtained via TokenService.</param>
        public async Task<string> SendAsync(
            string method,
            string path,
            IReadOnlyDictionary<string, string?>? query,
            object? body,
            string accessToken,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("method required", nameof(method));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path required", nameof(path));

            EnforceProdWriteGuard(method);

            var url = BuildUrl(path, query);
            using var request = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.Timeouts?.RequestTimeoutSeconds > 0
                ? _options.Timeouts.RequestTimeoutSeconds
                : 60);

            _logger.LogDebug("Aspire API: {Method} {Url}", method, url);
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // Log the full body for operators (server-side visibility) but keep the
                // exception message terse — exception messages may bubble up to MCP clients.
                _logger.LogWarning(
                    "Aspire API non-success: {Status} {Method} {Url} — body: {Body}",
                    (int)response.StatusCode, method, url, responseBody);
                throw new HttpRequestException(
                    $"Aspire API {method} {path} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            return responseBody;
        }

        private string BuildUrl(string path, IReadOnlyDictionary<string, string?>? query)
        {
            var baseUrl = (_options.BaseUrl ?? "https://cloud-api.youraspire.com").TrimEnd('/');
            var p = path.StartsWith('/') ? path : "/" + path;
            if (query is null || query.Count == 0) return baseUrl + p;
            var qs = string.Join("&", query
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
            return string.IsNullOrEmpty(qs) ? baseUrl + p : $"{baseUrl}{p}?{qs}";
        }

        // Hosts treated as sandbox/non-production. Anything else with a youraspire.com
        // suffix is treated as production. Unknown hosts (custom proxies, mocks, localhost)
        // are also treated as non-production — the guard exists to protect Aspire's prod
        // tenant, not arbitrary endpoints. Compared against the URL host only, so query
        // strings and path segments cannot influence the decision.
        private static readonly HashSet<string> SandboxHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "cloudsandbox-api.youraspire.com",
            "sandbox-api.youraspire.com",
        };
        private static readonly HashSet<string> ProductionHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "cloud-api.youraspire.com",
            "api.youraspire.com",
        };

        /// <summary>
        /// Refuse to send mutating requests at a known Aspire production host unless
        /// the operator has set ASPIRE_ALLOW_PROD_WRITES=1. Host membership is matched
        /// against an explicit allowlist — earlier substring matching ("sandbox") was
        /// fragile against adversarial hostnames like sandbox-prod.youraspire.com.
        /// </summary>
        private void EnforceProdWriteGuard(string method)
        {
            if (!MutatingMethods.Contains(method)) return;
            if (string.IsNullOrWhiteSpace(_options.BaseUrl)) return;
            if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var uri)) return;
            var host = uri.Host;
            if (SandboxHosts.Contains(host)) return;
            if (!ProductionHosts.Contains(host)) return; // unknown host => not Aspire prod
            var allow = Environment.GetEnvironmentVariable("ASPIRE_ALLOW_PROD_WRITES");
            if (string.Equals(allow, "1", StringComparison.Ordinal) ||
                string.Equals(allow, "true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            throw new InvalidOperationException(
                $"Refusing {method} request: host '{host}' is a known Aspire production endpoint and " +
                "ASPIRE_ALLOW_PROD_WRITES is not set to '1'. Point BaseUrl at the sandbox host " +
                "(cloudsandbox-api.youraspire.com) or set ASPIRE_ALLOW_PROD_WRITES=1 to authorize production writes.");
        }
    }
}
