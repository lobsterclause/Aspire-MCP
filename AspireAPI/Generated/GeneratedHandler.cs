using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Handlers;

namespace AspireAPI.Generated
{
    /// <summary>
    /// Base for every code-generated Aspire endpoint handler. Subclasses declare the
    /// method, path template, query-parameter names, path-parameter names, and whether
    /// they have a request body. Argument extraction and the actual HTTP send live here.
    /// </summary>
    public abstract class GeneratedHandler : BaseHandler
    {
        protected readonly AspireGenericClient Client;

        protected abstract string HttpMethod { get; }
        /// <summary>e.g. "/Contacts" or "/OpportunityTags/{id}".</summary>
        protected abstract string PathTemplate { get; }
        /// <summary>Names of substitutable {placeholders} in PathTemplate (case-sensitive).</summary>
        protected virtual IReadOnlyList<string> PathParameterNames => Array.Empty<string>();
        /// <summary>Names of accepted query parameters (case-insensitive lookup against tool args).</summary>
        protected virtual IReadOnlyList<string> QueryParameterNames => Array.Empty<string>();
        /// <summary>True if the operation accepts a JSON request body (POST/PUT/PATCH).</summary>
        protected virtual bool AcceptsBody => false;
        /// <summary>Argument key under which a request body object is supplied. Defaults to "body".</summary>
        protected virtual string BodyArgumentName => "body";

        // Pre-computed once per handler-type instance: union of path + query + body
        // names. Used to identify "everything else" as implicit body fields. Lazy-init
        // because PathParameterNames / QueryParameterNames are virtual and resolved
        // by the subclass, not the base.
        private HashSet<string>? _bodyIgnoreNames;
        private HashSet<string> BodyIgnoreNames => _bodyIgnoreNames ??= BuildBodyIgnoreNames();

        protected GeneratedHandler(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers)
        {
            Client = client;
        }

        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                // Upstream router (AspireMcpServer.CallToolHandlerAsync) constructs a
                // case-insensitive dictionary from the MCP request. We copy into our own
                // typed map (object? value type) to avoid forcing every callsite below
                // to disambiguate null. If `arguments` is null (per MCP spec for
                // parameterless tools) the empty static instance is reused.
                IDictionary<string, object?> args;
                if (arguments is null || arguments.Count == 0)
                {
                    args = EmptyArgs;
                }
                else
                {
                    var copy = new Dictionary<string, object?>(arguments.Count, StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in arguments) copy[kv.Key] = kv.Value;
                    args = copy;
                }

                var path = ResolvePath(args);
                var query = BuildQuery(args);
                var body = AcceptsBody ? ExtractBody(args) : null;

                var responseBody = await Client.SendAsync(
                    HttpMethod, path, query, body, accessToken, cancellationToken).ConfigureAwait(false);

                return CreateResponse(responseBody);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Honor cancellation — never wrap as a tool-level error.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Tool} ({Method} {Path})", GetType().Name, HttpMethod, PathTemplate);
                return new CallToolResponse().WithError($"{GetType().Name}: {ex.Message}");
            }
        }

        private string ResolvePath(IDictionary<string, object?> args)
        {
            var path = PathTemplate;
            foreach (var name in PathParameterNames)
            {
                if (!args.TryGetValue(name, out var value) || value is null)
                {
                    throw new ArgumentException($"Missing required path parameter '{name}' for {GetType().Name}.");
                }
                var literal = ToScalar(value);
                if (string.IsNullOrEmpty(literal))
                {
                    throw new ArgumentException($"Path parameter '{name}' must be non-empty for {GetType().Name}.");
                }
                path = path.Replace("{" + name + "}", Uri.EscapeDataString(literal));
            }
            return path;
        }

        private IReadOnlyDictionary<string, string?>? BuildQuery(IDictionary<string, object?> args)
        {
            if (QueryParameterNames.Count == 0) return null;
            var dict = new Dictionary<string, string?>();
            foreach (var name in QueryParameterNames)
            {
                if (args.TryGetValue(name, out var value) && value is not null)
                {
                    var scalar = ToScalar(value);
                    if (!string.IsNullOrEmpty(scalar)) dict[name] = scalar;
                }
            }
            return dict;
        }

        private object? ExtractBody(IDictionary<string, object?> args)
        {
            // First preference: explicit "body" argument.
            if (args.TryGetValue(BodyArgumentName, out var body) && body is not null) return body;
            // Otherwise: collect all unrecognized args into a body dictionary, using
            // the pre-computed ignore set so we don't allocate it per request.
            var ignore = BodyIgnoreNames;
            Dictionary<string, object?>? implicitBody = null;
            foreach (var kv in args)
            {
                if (kv.Value is null || ignore.Contains(kv.Key)) continue;
                (implicitBody ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase))[kv.Key] = kv.Value;
            }
            return implicitBody;
        }

        private HashSet<string> BuildBodyIgnoreNames()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var n in PathParameterNames) set.Add(n);
            foreach (var n in QueryParameterNames) set.Add(n);
            set.Add(BodyArgumentName);
            return set;
        }

        private static readonly Dictionary<string, object?> EmptyArgs =
            new(StringComparer.OrdinalIgnoreCase);

        private static string ToScalar(object value)
        {
            return value switch
            {
                string s => s,
                JsonElement el => el.ValueKind switch
                {
                    JsonValueKind.String => el.GetString() ?? string.Empty,
                    JsonValueKind.Number => el.GetRawText(),
                    JsonValueKind.True or JsonValueKind.False => el.GetBoolean() ? "true" : "false",
                    JsonValueKind.Null => string.Empty,
                    _ => el.GetRawText(),
                },
                _ => value.ToString() ?? string.Empty,
            };
        }
    }
}
