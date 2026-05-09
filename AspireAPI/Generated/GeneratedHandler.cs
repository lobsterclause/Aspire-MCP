using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
                var caseInsensitive = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                if (arguments != null)
                {
                    foreach (var kv in arguments) caseInsensitive[kv.Key] = kv.Value;
                }

                var path = ResolvePath(caseInsensitive);
                var query = BuildQuery(caseInsensitive);
                var body = AcceptsBody ? ExtractBody(caseInsensitive) : null;

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
            // Otherwise: collect all unrecognized scalar/object args into a body dictionary.
            var ignore = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var n in PathParameterNames) ignore.Add(n);
            foreach (var n in QueryParameterNames) ignore.Add(n);
            ignore.Add(BodyArgumentName);
            var implicitBody = args
                .Where(kv => !ignore.Contains(kv.Key) && kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            return implicitBody.Count == 0 ? null : implicitBody;
        }

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
