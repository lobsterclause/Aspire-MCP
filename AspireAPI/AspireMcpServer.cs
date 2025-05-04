using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
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
    
    // Inject all the required handlers
    public AspireMcpServer(
        TokenService tokenService,
        ILogger<AspireMcpServer> logger,
        IServiceProvider serviceProvider,
        ListPaymentsHandler listPaymentsHandler,
        ListPropertiesHandler listPropertiesHandler,
        ListContactsHandler listContactsHandler,
        ListJobsHandler listJobsHandler)
    {
        _tokenService = tokenService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _listPaymentsHandler = listPaymentsHandler;
        _listPropertiesHandler = listPropertiesHandler;
        _listContactsHandler = listContactsHandler;
        _listJobsHandler = listJobsHandler;

        // Create the tool router
        _toolRouter = new AspireToolRouter();

        // Register tools using the RegisterToolHandlers method from AspireServerTools
        RegisterToolHandlers();

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
        
        foreach (var toolDef in toolDefinitions)
        {
            var schema = await toolDef.GetSchemaAsync(cancellationToken);
            tools.Add(new Tool
            {
                Name = toolDef.Name,
                Description = toolDef.Description,
                InputSchema = schema.ToJson()
            });
        }

        return new ListToolsResult
        {
            Tools = tools
        };
    }

    /// <summary>
    /// Register tools explicitly (this is a backup method in case RegisterToolHandlers isn't called)
    /// </summary>
    private void RegisterTools()
    {
        // Register all activated tools
        _toolRouter.RegisterTool("ListPayments", provider =>
            _serviceProvider.GetRequiredService<ListPaymentsHandler>());
        _toolRouter.RegisterTool("ListProperties", provider =>
            _serviceProvider.GetRequiredService<ListPropertiesHandler>());
        _toolRouter.RegisterTool("ListContacts", provider =>
            _serviceProvider.GetRequiredService<ListContactsHandler>());
        _toolRouter.RegisterTool("ListJobs", provider =>
            _serviceProvider.GetRequiredService<ListJobsHandler>());
    }

    private async Task<CallToolResponse> CallToolHandlerAsync(CallToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var toolName = request.Params?.Name;
            var arguments = request.Params?.Arguments;

            if (string.IsNullOrEmpty(toolName) || arguments == null)
            {
                _logger.LogError("Invalid tool request: Missing tool name or arguments.");
                return new CallToolResponse().WithError("Invalid tool request");
            }

            // Use tool router to route to the appropriate handler
            var handler = _toolRouter.GetToolHandler(toolName, _serviceProvider);

            if (handler == null)
            {
                 _logger.LogError($"Unknown tool requested: {toolName}");
                 return new CallToolResponse().WithError($"Unknown tool: {toolName}");
            }

            // Convert arguments to IDictionary<string, object> for processing
            var argsJson = JsonSerializer.Serialize(arguments);
            var argsDictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(
                argsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (argsDictionary == null)
            {
                 _logger.LogError($"Failed to deserialize arguments for tool: {toolName}");
                 return new CallToolResponse().WithError("Failed to deserialize arguments");
            }

            // Get a valid access token
            string accessToken = await _tokenService.EnsureValidAsync(cancellationToken);

            // Call the handler with the correct parameter types
            var result = await handler.HandleAsync(argsDictionary, accessToken, cancellationToken);

            // Return the result directly as it's already a CallToolResponse
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling tool: {request.Params?.Name}");
            // Return error response instead of throwing McpServerException
            return new CallToolResponse().WithError($"Error calling tool: {ex.Message}");
        }
    }

    // Removed all other methods related to reporting, date calculations, data fetching, etc.
}