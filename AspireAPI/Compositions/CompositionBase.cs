using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;
using AspireAPI.Handlers;

namespace AspireAPI.Compositions
{
    /// <summary>
    /// Base for hand-written compositional tools that fan out across multiple
    /// generated endpoints and stitch the results into one LLM-friendly payload.
    /// Subclasses focus on the orchestration logic; auth/HTTP/error wrapping live here.
    /// </summary>
    public abstract class CompositionBase : BaseHandler
    {
        protected readonly AspireGenericClient Client;
        protected readonly TokenService TokenService;

        protected CompositionBase(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client,
            TokenService tokenService)
            : base(logger, httpClientFactory, apiHelpers)
        {
            Client = client;
            TokenService = tokenService;
        }

        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                var caseInsensitive = ToCaseInsensitive(arguments);
                var result = await ComposeAsync(caseInsensitive, accessToken, cancellationToken)
                    .ConfigureAwait(false);
                return CreateResponse(result.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Composition {Name} failed", GetType().Name);
                return new CallToolResponse().WithError($"{GetType().Name}: {ex.Message}");
            }
        }

        protected abstract Task<JsonNode> ComposeAsync(
            IReadOnlyDictionary<string, object?> args,
            string accessToken,
            CancellationToken cancellationToken);

        // ----- helpers used by subclasses -----

        protected static IReadOnlyDictionary<string, object?> ToCaseInsensitive(IDictionary<string, object>? args)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (args is not null)
            {
                foreach (var kv in args) dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        protected static string? GetString(IReadOnlyDictionary<string, object?> args, string key)
        {
            if (!args.TryGetValue(key, out var v) || v is null) return null;
            return v switch
            {
                string s => s,
                JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                _ => v.ToString(),
            };
        }

        protected static string Required(IReadOnlyDictionary<string, object?> args, string key)
        {
            var v = GetString(args, key);
            if (string.IsNullOrEmpty(v))
            {
                throw new ArgumentException($"Missing required argument '{key}'.");
            }
            return v!;
        }

        /// <summary>
        /// Issues a GET against /<resource> with $filter=<filter>, returns the parsed
        /// JSON body. Used by compositions to fan out across the API in parallel.
        /// </summary>
        protected async Task<JsonNode?> ListWithFilterAsync(
            string resource, string filter, string accessToken,
            CancellationToken cancellationToken,
            int top = 200)
        {
            var query = new Dictionary<string, string?>
            {
                ["$filter"] = filter,
                ["$top"] = top.ToString(),
            };
            var body = await Client.SendAsync("GET", "/" + resource, query, body: null,
                accessToken: accessToken, cancellationToken: cancellationToken).ConfigureAwait(false);
            return JsonNode.Parse(body);
        }

        protected async Task<JsonNode?> GetAsync(
            string path, string accessToken, CancellationToken cancellationToken)
        {
            var body = await Client.SendAsync("GET", path, query: null, body: null,
                accessToken: accessToken, cancellationToken: cancellationToken).ConfigureAwait(false);
            return JsonNode.Parse(body);
        }

        /// <summary>
        /// Wraps a call so a 4xx/5xx becomes a {"error": "..."} node rather than throwing.
        /// Compositions surface partial results when one branch fails — better than the
        /// whole composition aborting.
        /// </summary>
        protected static async Task<JsonNode?> SafeAsync(Func<Task<JsonNode?>> fetch)
        {
            try { return await fetch().ConfigureAwait(false); }
            catch (Exception ex) { return new JsonObject { ["error"] = ex.Message }; }
        }
    }
}
