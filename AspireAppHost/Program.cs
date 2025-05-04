using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add the MCP Inspector with Stdio transport
// This exposes the Model Context Protocol server through a standardized endpoint
builder.AddMCPInspector("aspire-mcp-server").WithStdio<Projects.AspireAPI>();

// Add environment variables needed for the MCP server to connect to Aspire Cloud API
builder.AddEnvironmentVariables("ASPIRE__BASE_URL", "ASPIRE__USERNAME", "ASPIRE__PASSWORD", "ASPIRE__COMPANYKEY");

// Build and run the application
var app = builder.Build();
await app.RunAsync();
