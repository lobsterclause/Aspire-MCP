using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using AspireAPI.AdminWeb;
using AspireAPI.Handlers; // Ensure Handlers namespace is included
using AspireAPI.ToolDefinitions; // Ensure ToolDefinitions namespace is included
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;

namespace AspireAPI;
public partial class AspireMcpServer
{
    private readonly IMcpServer _server;
    private readonly TokenService _tokenService;
    private readonly ILogger<AspireMcpServer> _logger;
    private readonly AspireToolRouter _toolRouter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ListPaymentsHandler _listPaymentsHandler;
    private readonly ListPropertiesHandler _listPropertiesHandler;
    private readonly ListContactsHandler _listContactsHandler;
    private readonly ListJobsHandler _listJobsHandler;
    
    // Router is now resolved from DI (singleton) — admin-mode UI shares the same
    // router instance so its catalog/runner sees the exact same tool surface as
    // an MCP client would.
    private readonly ToolAllowlistStore _allowlist;
    private readonly CallTailBuffer _tail;

    public AspireMcpServer(
        TokenService tokenService,
        ILogger<AspireMcpServer> logger,
        IServiceProvider serviceProvider,
        AspireToolRouter toolRouter,
        ToolAllowlistStore allowlist,
        CallTailBuffer tail,
        ListPaymentsHandler listPaymentsHandler,
        ListPropertiesHandler listPropertiesHandler,
        ListContactsHandler listContactsHandler,
        ListJobsHandler listJobsHandler)
    {
        _tokenService = tokenService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _toolRouter = toolRouter;
        _allowlist = allowlist;
        _tail = tail;
        _listPaymentsHandler = listPaymentsHandler;
        _listPropertiesHandler = listPropertiesHandler;
        _listContactsHandler = listContactsHandler;
        _listJobsHandler = listJobsHandler;

        // Create the MCP server with Stdio transport
        var options = new McpServerOptions
        {
            ServerInfo = new ServerInfo
            {
                Name = "aspire-mcp-server",
                Version = "1.0.0",
                Description = "MCP Server for Aspire Cloud API"
            },
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapabilities
                {
                    ListToolsHandler = ListToolsHandlerAsync,
                    CallToolHandler = CallToolHandlerAsync
                }
            }
        };

        _server = McpServer.Create(new StdioServerTransport(), options);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _server.StopAsync(cancellationToken);
    }

    private async Task<ListToolsResult> ListToolsHandlerAsync(ListToolsRequest request, CancellationToken cancellationToken)
    {
        // Get all registered tool definitions
        var toolDefinitions = _serviceProvider.GetServices<IToolDefinition>().ToList();

        if (toolDefinitions == null || !toolDefinitions.Any())
        {
             _logger.LogError("No tool definitions found in service provider.");
             return new ListToolsResult { Tools = new List<Tool>() };
        }

        var tools = new List<Tool>();
        
        // Reload allowlist so out-of-band edits to Local.json are picked up
        // without restart. Cheap (one file stat + small parse).
        _allowlist.Load();

        foreach (var toolDef in toolDefinitions)
        {
            // Skip tools the operator has disabled — they shouldn't appear in
            // tools/list either, otherwise clients keep trying to call them.
            if (!_allowlist.IsAllowed(toolDef.Name)) continue;

            var schema = await toolDef.GetSchemaAsync(cancellationToken);
            // Per the MCP spec, inputSchema is a nested JSON object on the wire,
            // not a JSON string. Parse the rendered schema once here so the
            // outer JSON-RPC serializer embeds it as an object.
            using var schemaDoc = JsonDocument.Parse(schema.ToJson());
            tools.Add(new Tool
            {
                Name = toolDef.Name,
                Description = toolDef.Description,
                InputSchema = schemaDoc.RootElement.Clone()
            });
        }

        return new ListToolsResult
        {
            Tools = tools
        };
    }

    private async Task<CallToolResponse> CallToolHandlerAsync(CallToolRequest request, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var toolName = request.Params?.Name ?? "<unknown>";
        try
        {
            var arguments = request.Params?.Arguments;

            if (string.IsNullOrEmpty(request.Params?.Name))
            {
                _logger.LogError("Invalid tool request: missing tool name.");
                return new CallToolResponse().WithError("Invalid tool request");
            }

            // Refuse explicitly disabled tools — even if a client kept the name
            // from a previous tools/list call.
            _allowlist.Load();
            if (!_allowlist.IsAllowed(toolName))
            {
                _tail.Record(new CallEntry(DateTime.UtcNow, toolName, false, 0, 403, false,
                    "tool disabled by allowlist", "mcp"));
                return new CallToolResponse().WithError($"Tool '{toolName}' is disabled.");
            }

            // Use tool router to route to the appropriate handler
            var handler = _toolRouter.GetToolHandler(toolName, _serviceProvider);

            if (handler == null)
            {
                 _logger.LogError($"Unknown tool requested: {toolName}");
                 _tail.Record(new CallEntry(DateTime.UtcNow, toolName, false, 0, 404, false,
                     "unknown tool", "mcp"));
                 return new CallToolResponse().WithError($"Unknown tool: {toolName}");
            }

            // Per the MCP spec, parameterless tools may be invoked with no arguments
            // (omitted or null). Treat null as the empty argument set.
            // Avoid the Serialize→Deserialize string round-trip on the hot path:
            // if `arguments` is already a JsonElement (the common SDK shape), parse
            // it directly. Otherwise fall back to the round-trip for unfamiliar
            // shapes coming from custom transports.
            IDictionary<string, object> argsDictionary;
            if (arguments is null)
            {
                argsDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                if (arguments is JsonElement element)
                {
                    argsDictionary = element.ValueKind == JsonValueKind.Object
                        ? element.Deserialize<IDictionary<string, object>>(deserializeOptions)
                          ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    var argsJson = JsonSerializer.Serialize(arguments);
                    argsDictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(
                        argsJson, deserializeOptions)
                        ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
            }

            // Get a valid access token
            string accessToken = await _tokenService.EnsureValidAsync(cancellationToken);

            // Call the handler with the correct parameter types
            var result = await handler.HandleAsync(argsDictionary, accessToken, cancellationToken);
            sw.Stop();
            _tail.Record(new CallEntry(DateTime.UtcNow, toolName, result.Error is null,
                sw.ElapsedMilliseconds, result.Error is null ? 200 : 500, false,
                result.Error?.Message, "mcp"));
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Honor MCP cancellation — bubble up to the JSON-RPC layer instead of
            // wrapping it as a tool error.
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, $"Error handling tool: {toolName}");
            _tail.Record(new CallEntry(DateTime.UtcNow, toolName, false,
                sw.ElapsedMilliseconds, 500, false, ex.Message, "mcp"));
            return new CallToolResponse().WithError($"Error calling tool: {ex.Message}");
        }
    }

    // Removed all other methods related to reporting, date calculations, data fetching, etc.
}