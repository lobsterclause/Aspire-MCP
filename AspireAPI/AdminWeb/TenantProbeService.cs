using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.AdminWeb
{
    /// <summary>
    /// "Smart" allowlist seeding: probe every safe-to-poll Aspire endpoint with
    /// $top=1 and classify the result. Lets the operator turn the 162-tool
    /// surface into a tenant-tailored subset without manually toggling 60
    /// checkboxes.
    ///
    /// Probed: every code-generated tool whose method is GET *and* has no path
    /// parameters (i.e. collection list endpoints). Skipped: writes
    /// (POST/PUT/PATCH/DELETE — could create data), GET-by-id (no id to
    /// supply), hand-written compositions (already curated), SearchAspire
    /// (depends on Anthropic, separate concern).
    ///
    /// Classifications:
    ///   populated   — 200, body contained at least one record. Recommend ENABLE.
    ///   empty       — 200 but no records. Recommend DISABLE (toggleable).
    ///   auth_failed — 401/403. The configured API client doesn't have scope.
    ///                 Recommend DISABLE; surface so operator can grant scope.
    ///   broken      — any other non-2xx. Recommend DISABLE; surface error body.
    ///   timeout     — request didn't complete in budget. Recommend DISABLE.
    /// </summary>
    public sealed class TenantProbeService
    {
        private const int ProbeConcurrency = 8;
        private const int ProbeTimeoutSeconds = 15;

        private readonly AspireGenericClient _client;
        private readonly TokenService _tokenService;
        private readonly ILogger<TenantProbeService> _logger;
        private readonly IReadOnlyList<ProbeCandidate> _candidates;

        public TenantProbeService(
            AspireGenericClient client,
            TokenService tokenService,
            ILogger<TenantProbeService> logger)
        {
            _client = client;
            _tokenService = tokenService;
            _logger = logger;
            _candidates = LoadCandidates();
        }

        public IReadOnlyList<string> ProbableToolNames => _candidates.Select(c => c.Tool).ToArray();

        public async Task<ProbeRunResult> ProbeAsync(CancellationToken cancellationToken)
        {
            string token;
            try
            {
                token = await _tokenService.EnsureValidAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ProbeRunResult(
                    StartedAtUtc: DateTime.UtcNow,
                    DurationMs: 0,
                    Probed: 0,
                    Results: Array.Empty<ProbeResult>(),
                    FatalError: $"Could not acquire Aspire access token: {ex.Message}");
            }

            var sw = Stopwatch.StartNew();
            var startedAt = DateTime.UtcNow;
            using var gate = new SemaphoreSlim(ProbeConcurrency);
            var tasks = _candidates.Select(c => RunOneAsync(c, token, gate, cancellationToken)).ToArray();
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            sw.Stop();

            return new ProbeRunResult(
                StartedAtUtc: startedAt,
                DurationMs: sw.ElapsedMilliseconds,
                Probed: results.Length,
                Results: results.OrderBy(r => r.Tool, StringComparer.OrdinalIgnoreCase).ToArray(),
                FatalError: null);
        }

        private async Task<ProbeResult> RunOneAsync(
            ProbeCandidate candidate, string token, SemaphoreSlim gate, CancellationToken parent)
        {
            await gate.WaitAsync(parent).ConfigureAwait(false);
            var sw = Stopwatch.StartNew();
            try
            {
                using var localCts = CancellationTokenSource.CreateLinkedTokenSource(parent);
                localCts.CancelAfter(TimeSpan.FromSeconds(ProbeTimeoutSeconds));

                // $top=1 keeps the payload tiny — we just need to know "is anything there".
                var query = new Dictionary<string, string?> { ["$top"] = "1" };
                string body;
                try
                {
                    body = await _client.SendAsync(
                        "GET", candidate.Path, query, body: null, accessToken: token,
                        cancellationToken: localCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!parent.IsCancellationRequested)
                {
                    sw.Stop();
                    return new ProbeResult(candidate.Tool, candidate.Path,
                        Classification.Timeout, null, sw.ElapsedMilliseconds,
                        Recommend: false, Note: $"Timed out after {ProbeTimeoutSeconds}s");
                }
                catch (HttpRequestException ex)
                {
                    sw.Stop();
                    var (cls, code, note) = ClassifyHttpException(ex);
                    return new ProbeResult(candidate.Tool, candidate.Path, cls, code, sw.ElapsedMilliseconds,
                        Recommend: false, Note: note);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new ProbeResult(candidate.Tool, candidate.Path,
                        Classification.Broken, null, sw.ElapsedMilliseconds,
                        Recommend: false, Note: ex.Message);
                }
                sw.Stop();

                var hasData = BodyHasData(body);
                return new ProbeResult(candidate.Tool, candidate.Path,
                    hasData ? Classification.Populated : Classification.Empty,
                    StatusCode: 200,
                    DurationMs: sw.ElapsedMilliseconds,
                    Recommend: hasData,
                    Note: hasData ? null : "No records returned");
            }
            finally { gate.Release(); }
        }

        // The exception message from AspireGenericClient is shaped:
        //   "Aspire API GET /Foo returned 401 Unauthorized: …"
        // Walk that to extract the status code.
        private static (Classification, int?, string?) ClassifyHttpException(HttpRequestException ex)
        {
            var msg = ex.Message ?? "";
            int? code = null;
            foreach (var token in msg.Split(' '))
            {
                if (token.Length == 3 && int.TryParse(token, out var c) && c >= 100 && c < 600)
                {
                    code = c;
                    break;
                }
            }
            if (code is 401 or 403)
            {
                return (Classification.AuthFailed, code, "API client lacks scope for this endpoint");
            }
            return (Classification.Broken, code, msg);
        }

        // Aspire returns either {"data": [...], "totalCount": N} for collections or
        // sometimes a bare array. Treat either non-empty array as "has data".
        private static bool BodyHasData(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return false;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    return root.GetArrayLength() > 0;
                }
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                    {
                        return data.GetArrayLength() > 0;
                    }
                    if (root.TryGetProperty("Data", out var data2) && data2.ValueKind == JsonValueKind.Array)
                    {
                        return data2.GetArrayLength() > 0;
                    }
                    // Fallback: assume any non-empty object is "has data".
                    return root.EnumerateObject().Any();
                }
            }
            catch
            {
                // Unparseable — caller will decide what to do; treat as no-data.
                return false;
            }
            return false;
        }

        private static IReadOnlyList<ProbeCandidate> LoadCandidates()
        {
            // tool-manifest.json is committed alongside the generated handlers and
            // embedded into the assembly via .csproj. Reading it here means the
            // probe service auto-picks up new collection endpoints as the codegen
            // runs against fresh swagger pulls.
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("AspireAPI.Generated.tool-manifest.json")
                ?? throw new InvalidOperationException("tool-manifest.json embedded resource not found");
            using var reader = new StreamReader(stream);
            using var doc = JsonDocument.Parse(reader.ReadToEnd());
            var tools = doc.RootElement.GetProperty("tools");
            var list = new List<ProbeCandidate>();
            foreach (var t in tools.EnumerateArray())
            {
                var method = t.GetProperty("method").GetString();
                if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase)) continue;
                var pathParams = t.GetProperty("pathParams");
                if (pathParams.GetArrayLength() > 0) continue; // skip GET-by-id
                list.Add(new ProbeCandidate(
                    Tool: t.GetProperty("name").GetString()!,
                    Path: t.GetProperty("path").GetString()!));
            }
            return list;
        }

        private sealed record ProbeCandidate(string Tool, string Path);
    }

    public enum Classification { Populated, Empty, AuthFailed, Broken, Timeout }

    public sealed record ProbeResult(
        string Tool,
        string Path,
        Classification Classification,
        int? StatusCode,
        long DurationMs,
        bool Recommend,
        string? Note);

    public sealed record ProbeRunResult(
        DateTime StartedAtUtc,
        long DurationMs,
        int Probed,
        IReadOnlyList<ProbeResult> Results,
        string? FatalError);
}
