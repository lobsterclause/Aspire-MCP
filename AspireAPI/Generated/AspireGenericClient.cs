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
    /// returns the response body. Mutating methods consult the production-write guard,
    /// which is enforced against the *resolved* request host (not just the configured
    /// BaseUrl) so that a missing-config default cannot silently route writes to prod.
    /// Binary responses are returned base64-encoded under a JSON envelope so callers
    /// can distinguish them from text payloads.
    /// </summary>
    public sealed class AspireGenericClient
    {
        // Production base URL Aspire returns from its swagger UI link. Used as the
        // default when no BaseUrl is configured. The same default lives in the
        // production-write guard so that "no config" still means "no silent writes."
        internal const string DefaultProductionBaseUrl = "https://cloud-api.youraspire.com";

        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "POST", "PUT", "PATCH", "DELETE"
        };

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

        // Content types we treat as text (return body as string). Anything else is
        // returned base64-encoded so binary endpoints (file downloads, attachments)
        // don't get UTF-8-corrupted into "" replacement characters.
        private static readonly string[] TextMediaTypePrefixes =
        {
            "application/json", "application/problem+json",
            "application/xml", "application/x-www-form-urlencoded",
            "text/",
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

            var normalizedMethod = method.ToUpperInvariant();
            var url = BuildUrl(path, query);

            // Guard runs against the *resolved* URL so the BaseUrl-default-to-prod
            // path can never bypass it. (If BaseUrl is unset, BuildUrl returns
            // DefaultProductionBaseUrl + path, and the guard sees that host.)
            EnforceProdWriteGuard(normalizedMethod, url);

            using var request = new HttpRequestMessage(new HttpMethod(normalizedMethod), url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            // Per-request timeout via a linked CancellationTokenSource — never mutate
            // HttpClient.Timeout on a factory-pooled instance (race condition risk).
            var timeoutSeconds = _options.Timeouts?.RequestTimeoutSeconds is > 0
                ? _options.Timeouts.RequestTimeoutSeconds
                : 60;
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var client = _httpClientFactory.CreateClient();

            _logger.LogDebug("Aspire API: {Method} {Url}", normalizedMethod, url);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            var responseBody = await ReadBodyAsync(response, linkedCts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Aspire API non-success: {Status} {Method} {Url} — body: {Body}",
                    (int)response.StatusCode, normalizedMethod, url, responseBody);
                throw new HttpRequestException(
                    $"Aspire API {normalizedMethod} {path} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            return responseBody;
        }

        private async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var isText = mediaType.Length == 0 || TextMediaTypePrefixes.Any(p =>
                mediaType.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (isText)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }

            // Non-text payload (e.g. /Attachments/AttachmentFileData returns raw file bytes).
            // Wrap in a JSON envelope so callers can detect + decode.
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var envelope = new
            {
                contentType = mediaType,
                contentLength = bytes.LongLength,
                base64 = Convert.ToBase64String(bytes),
            };
            return JsonSerializer.Serialize(envelope);
        }

        private string BuildUrl(string path, IReadOnlyDictionary<string, string?>? query)
        {
            var baseUrl = (_options.BaseUrl ?? DefaultProductionBaseUrl).TrimEnd('/');
            var p = path.StartsWith('/') ? path : "/" + path;
            if (query is null || query.Count == 0) return baseUrl + p;
            var qs = string.Join("&", query
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
            return string.IsNullOrEmpty(qs) ? baseUrl + p : $"{baseUrl}{p}?{qs}";
        }

        /// <summary>
        /// Refuse to send mutating requests at a known Aspire production host unless
        /// the operator has set ASPIRE_ALLOW_PROD_WRITES=1. Evaluated against the
        /// resolved request URL — not the raw BaseUrl option — so that the implicit
        /// production default that BuildUrl applies cannot bypass the guard.
        /// </summary>
        private static void EnforceProdWriteGuard(string normalizedMethod, string resolvedUrl)
        {
            if (!MutatingMethods.Contains(normalizedMethod)) return;
            if (!Uri.TryCreate(resolvedUrl, UriKind.Absolute, out var uri)) return;
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
                $"Refusing {normalizedMethod} request: host '{host}' is a known Aspire production endpoint and " +
                "ASPIRE_ALLOW_PROD_WRITES is not set to '1'. Point BaseUrl at the sandbox host " +
                "(cloudsandbox-api.youraspire.com) or set ASPIRE_ALLOW_PROD_WRITES=1 to authorize production writes.");
        }
    }
}
