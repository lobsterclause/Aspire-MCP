using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;

namespace AspireAPI
{
    // Simplified implementation of server interfaces to make the build work
    public interface IMcpServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
    
    public class StdioServerTransport { }
    
    public class McpServer : IMcpServer
    {
        public static IMcpServer Create(StdioServerTransport transport, McpServerOptions options)
        {
            return new McpServer();
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
    public class McpServerOptions
    {
        public ServerInfo ServerInfo { get; set; } = new ServerInfo();
        public ServerCapabilities Capabilities { get; set; } = new ServerCapabilities();
    }
    
    public class ServerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    
    public class ServerCapabilities
    {
        public ToolsCapabilities Tools { get; set; } = new ToolsCapabilities();
    }
    
    public class ToolsCapabilities
    {
        public ListToolsHandlerType ListToolsHandler { get; set; }
        public CallToolHandlerType CallToolHandler { get; set; }
    }
    
    public delegate Task<ListToolsResult> ListToolsHandlerType(ListToolsRequest request, CancellationToken cancellationToken);
    public delegate Task<CallToolResponse> CallToolHandlerType(CallToolRequest request, CancellationToken cancellationToken);
}