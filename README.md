# AspireMCP Server

A Model Context Protocol (MCP) server implementation for Aspire API integration that enables AI assistants to communicate with Aspire APIs through a standardized protocol.

## Project Overview

The AspireMCP server exposes Aspire API functionality as tools and resources that can be used by AI assistants. It implements the Model Context Protocol to standardize communication between AI models and Aspire systems.

### Key Components

- **AspireAPI**: Core project containing the MCP server implementation and API handlers
- **TestScripts**: Node.js based testing scripts to verify server functionality

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Node.js](https://nodejs.org/) v12 or higher (for running test scripts)
- npm or yarn package manager (for managing test dependencies)

## Building the AspireAPI Project

The AspireAPI project can be built using the .NET CLI:

```bash
# Navigate to the AspireAPI directory
cd AspireAPI

# Build the project
dotnet build

# Optional: Publish the project for deployment
dotnet publish -c Release -o ./publish
```

## Starting the AspireMCP Server

After building, you can start the AspireMCP server:

```bash
# Navigate to the AspireAPI directory
cd AspireAPI

# Run the server
dotnet run
```

By default, the server starts listening on `http://localhost:5000/mcp`. The server uses the Stdio transport for Model Context Protocol communication.

## Testing the AspireMCP Server

### Manual Testing

1. Start the AspireMCP server as described above
2. In a separate terminal, run the test scripts:

```bash
# Navigate to the TestScripts directory
cd TestScripts

# Install dependencies (first time only)
npm install

# Run the test script
node test-mcp-server.js
```

### Using the Automated Script

This repository includes automated scripts that build the project, start the server, run tests, and report results:

- For Windows: `run-aspire-mcp.bat`
- For Linux/macOS: `run-aspire-mcp.sh`

To use the script:

```bash
# Windows
.\run-aspire-mcp.bat

# Linux/macOS
chmod +x run-aspire-mcp.sh
./run-aspire-mcp.sh
```

## Test Script Options

The test script accepts the following command-line options:

- `--server=URL`: Specify the MCP server URL (default: `http://localhost:5000/mcp`)
- `--tool=TOOL_NAME`: Specify which tool to test (default: `ListPayments`)

Example:

```bash
node test-mcp-server.js --server=http://localhost:5050/mcp
```

## Understanding Test Results

The test script provides color-coded output to identify:

- ✓ Green text: successful operations
- ✗ Red text: failed operations
- ⚠ Yellow text: warnings or information
- Gray text: additional details

A test summary is displayed at the end, indicating which tests passed or failed.

## Troubleshooting

### Connection Issues

If you cannot connect to the AspireMCP server:

1. Ensure the server is running in a separate terminal
2. Verify the server URL is correct (default: `http://localhost:5000/mcp`)
3. Check if any firewall is blocking the connection

### Build Errors

If you experience build errors:

1. Verify you have .NET 8.0 SDK installed: `dotnet --version`
2. Restore packages: `dotnet restore`
3. Clean the solution: `dotnet clean` and rebuild

### Test Execution Failures

If the tests fail:

1. Check the AspireMCP server logs for errors
2. Verify that the required tools (e.g., ListPayments) are properly registered in the server
3. Ensure the tool parameters are valid
4. For advanced query failures, verify that the OData query syntax is correct

## Project Structure

```
AspireMCP/
├── AspireAPI/              # Core MCP server implementation
│   ├── AspireMcpServer.cs  # Main MCP server class
│   ├── AspireServerTools.cs # Tool definitions and handlers
│   └── [Other components]  # Additional server components
├── TestScripts/            # Node.js test scripts
│   ├── test-mcp-server.js  # Main test script
│   └── package.json        # Node.js dependencies
├── run-aspire-mcp.sh       # Linux/macOS automation script
└── run-aspire-mcp.bat      # Windows automation script
```

## Advanced Usage

### Custom Tool Development

To add new tools to the AspireMCP server:

1. Create a new tool definition in `AspireAPI/ToolDefinitions/`
2. Implement the handler in `AspireAPI/Handlers/`
3. Register the tool in `AspireAPI/AspireMcpServer.cs`

### Server Configuration

The AspireMCP server can be configured through `appsettings.json` for:

- API endpoints
- Authentication settings
- Logging configuration
- Tool-specific parameters