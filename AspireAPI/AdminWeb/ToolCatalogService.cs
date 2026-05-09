using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AspireAPI.Generated;

namespace AspireAPI.AdminWeb
{
    /// <summary>
    /// Discovery + invocation surface that the admin UI sits on top of.
    /// Shares the singleton AspireToolRouter with the stdio MCP server, so
    /// what the operator sees in the catalog is exactly what an MCP client
    /// would see in tools/list.
    /// </summary>
    public sealed class ToolCatalogService
    {
        private readonly IServiceProvider _provider;
        private readonly AspireToolRouter _router;
        private readonly TokenService _tokenService;
        private readonly IEnumerable<IToolDefinition> _toolDefinitions;

        public ToolCatalogService(
            IServiceProvider provider,
            AspireToolRouter router,
            TokenService tokenService,
            IEnumerable<IToolDefinition> toolDefinitions)
        {
            _provider = provider;
            _router = router;
            _tokenService = tokenService;
            _toolDefinitions = toolDefinitions;
        }

        /// <summary>
        /// Returns metadata + JSON schema for every registered tool, sorted by name.
        /// </summary>
        public async Task<IReadOnlyList<ToolCatalogEntry>> ListAsync(CancellationToken cancellationToken)
        {
            var entries = new List<ToolCatalogEntry>();
            foreach (var def in _toolDefinitions.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
            {
                JsonNode? schemaNode;
                try
                {
                    var schema = await def.GetSchemaAsync(cancellationToken).ConfigureAwait(false);
                    schemaNode = JsonNode.Parse(schema.ToJson());
                }
                catch (Exception ex)
                {
                    schemaNode = new JsonObject { ["__schemaError"] = ex.Message };
                }
                entries.Add(new ToolCatalogEntry(def.Name, def.Description, schemaNode));
            }
            return entries;
        }

        /// <summary>
        /// Invokes a tool, optionally in dry-run mode (returns the would-be
        /// HTTP request without actually sending it). Always uses the same
        /// argument-translation path as the stdio MCP server.
        /// </summary>
        public async Task<ToolInvocationResult> InvokeAsync(
            string toolName,
            JsonElement? arguments,
            bool dryRun,
            CancellationToken cancellationToken)
        {
            var handler = _router.GetToolHandler(toolName, _provider);
            if (handler is null)
            {
                return new ToolInvocationResult(
                    Ok: false,
                    StatusCode: 404,
                    DurationMs: 0,
                    Body: $"Unknown tool '{toolName}'.",
                    DryRun: dryRun);
            }

            // Same argument shape as AspireMcpServer.CallToolHandlerAsync.
            IDictionary<string, object> args;
            if (arguments is null || arguments.Value.ValueKind == JsonValueKind.Null
                || arguments.Value.ValueKind == JsonValueKind.Undefined)
            {
                args = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else if (arguments.Value.ValueKind == JsonValueKind.Object)
            {
                args = arguments.Value.Deserialize<IDictionary<string, object>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                return new ToolInvocationResult(
                    Ok: false,
                    StatusCode: 400,
                    DurationMs: 0,
                    Body: "arguments must be a JSON object",
                    DryRun: dryRun);
            }

            if (dryRun)
            {
                // Return a stub response describing what would be sent. We don't
                // execute the handler — too easy to forget to set ASPIRE_ALLOW_PROD_WRITES
                // and end up firing real writes from the catalog runner.
                return new ToolInvocationResult(
                    Ok: true,
                    StatusCode: 0,
                    DurationMs: 0,
                    Body: JsonSerializer.Serialize(new
                    {
                        dryRun = true,
                        tool = toolName,
                        argumentsResolved = args,
                        note = "Dry-run: handler not invoked. Re-run with dryRun=false to actually call Aspire.",
                    }, new JsonSerializerOptions { WriteIndented = true }),
                    DryRun: true);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var token = await _tokenService.EnsureValidAsync(cancellationToken).ConfigureAwait(false);
                var response = await handler.HandleAsync(args, token, cancellationToken).ConfigureAwait(false);
                sw.Stop();

                var ok = response.Error is null;
                var body = ok ? ContentToString(response.Content) : (response.Error?.Message ?? string.Empty);
                return new ToolInvocationResult(
                    Ok: ok,
                    StatusCode: ok ? 200 : 500,
                    DurationMs: sw.ElapsedMilliseconds,
                    Body: body,
                    DryRun: false);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new ToolInvocationResult(
                    Ok: false,
                    StatusCode: 500,
                    DurationMs: sw.ElapsedMilliseconds,
                    Body: ex.Message,
                    DryRun: false);
            }
        }

        private static string ContentToString(object? content)
        {
            if (content is null) return string.Empty;
            if (content is string s) return s;
            // CallToolResponse.WithContent stores a Dictionary<string,object> { "text": "..." }.
            if (content is IDictionary<string, object> dict && dict.TryGetValue("text", out var t))
            {
                return t?.ToString() ?? string.Empty;
            }
            return JsonSerializer.Serialize(content);
        }
    }

    public sealed record ToolCatalogEntry(string Name, string Description, JsonNode? Schema);

    public sealed record ToolInvocationResult(
        bool Ok,
        int StatusCode,
        long DurationMs,
        string Body,
        bool DryRun);
}
