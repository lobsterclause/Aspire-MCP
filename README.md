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
- An Aspire tenant with API access enabled (sandbox by default — see below)

## Configuring Aspire credentials

Before the server can call any tool, you must populate `appsettings.Development.json`
(or `appsettings.json`, or environment variables) with OAuth credentials issued
by your Aspire administrator.

### One-time setup in the Aspire UI

In the Aspire web app, navigate to: **Administration → Application → API**.
There, **Create Client** to mint a `ClientId` / `ClientSecret` pair, choose the
scopes the integration needs, and toggle **Active** on. The same screen lists
the username + company key the OAuth flow needs.

### Option A: web admin UI (recommended)

Run the binary in admin mode and edit settings in your browser:

```bash
cd AspireAPI
dotnet run -- --admin           # binds 127.0.0.1:5050 by default
# or:  dotnet run -- --admin --port 5060
```

Open <http://127.0.0.1:5050/admin>. The UI writes
`AspireAPI/appsettings.Local.json` (gitignored). The MCP server reads it on
next launch — no restart of the admin process is needed; restart your
**MCP client** (e.g. Claude Desktop) so it relaunches the stdio binary
with the new config.

The admin UI is local-only by design (binds 127.0.0.1, no auth) and must
never be exposed on a network you don't trust.

### Option B: hand-edit appsettings.Local.json

If you'd rather skip the UI, create `AspireAPI/appsettings.Local.json`
(gitignored — never commit credentials):

```jsonc
{
  "AspireApi": {
    // Sandbox is the default — change to https://cloud-api.youraspire.com for prod
    "BaseUrl": "https://cloudsandbox-api.youraspire.com",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "OAuthServerUrl": "https://cloudsandbox-api.youraspire.com",
    "Auth": {
      "Username": "your-aspire-user@example.com",
      "Password": "...",
      "CompanyKey": "your-company-key"
    }
  }
}
```

### Production-write safety

The MCP server refuses POST/PUT/PATCH/DELETE requests at the production host
(`cloud-api.youraspire.com` or `api.youraspire.com`) unless the operator sets
`ASPIRE_ALLOW_PROD_WRITES=1` in the environment. This is intentional — point
`BaseUrl` at the sandbox for routine development. Reads are always allowed.

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

## Tool surface

As of the most recent codegen run (Aspire OpenAPI spec dated Nov 2025), the
server *registers* **163 tools**: 158 code-generated (one per `(path, method)`
pair across the entire Aspire External REST API) + 5 hand-written
compositions (`GetJobLifecycle`, `GetCustomer360`, `RenderScheduleBoard`,
`ListChangedSince`, `SearchAspire`).

**Discovery-first default.** A fresh install only *exposes* 6 tools to MCP
clients: the 5 compositions plus `GetVersionGetApiVersion`. Everything else
is registered but hidden by the allowlist. The intent is that a freshly-
connected Claude (or any MCP client) sees a small focused surface and can
route most queries through `SearchAspire` until the operator has tailored
the catalog to their tenant.

To unlock more of the catalog, run **Auto-detect from tenant** in the admin
UI — it probes every safe collection endpoint with `$top=1`, classifies the
result (populated / empty / auth-failed / broken), and the operator picks
which to expose. Apply switches the allowlist into Allowlist mode with the
opted-in set + the discovery bootstrap.

The full registered inventory — names, methods, paths, query/path
parameters, and descriptions — is committed at
`AspireAPI/Generated/tool-manifest.json`.

### Production-write safety guard

Mutating verbs (POST/PUT/PATCH/DELETE) are silently refused when the configured
`AspireApi:BaseUrl` points at the production host (anything not containing
"sandbox") unless `ASPIRE_ALLOW_PROD_WRITES=1` is set in the environment.
Point at `https://cloudsandbox-api.youraspire.com` for routine development.

### Regenerating the tool surface

When Aspire publishes new endpoints (see
https://guide.youraspire.com/v1-api/apidocs/whats-new ), refresh the spec and
re-run codegen:

```bash
curl -sL https://cloud-api.youraspire.com/swagger/v1/swagger.json \
  -o tools/aspire-codegen/swagger.json
python3 tools/aspire-codegen/generate.py
dotnet build AspireAPI/AspireAPI.csproj
```

Generated files live under `AspireAPI/Generated/` and are intentionally
committed for auditability — diffs against them on a swagger refresh are how
you see what Aspire added or removed.

## Advanced Usage

### Custom Tool Development

Most tools should be added by re-running the codegen against an updated swagger
spec (see above). For genuinely bespoke tools that don't map 1:1 to an Aspire
endpoint, add a hand-written pair under `AspireAPI/Handlers/` +
`AspireAPI/ToolDefinitions/` and register them in
`AspireAPI/AspireServerTools.cs::RegisterToolHandlers`.

### Server Configuration

The AspireMCP server can be configured through `appsettings.json` for:

- API endpoints
- Authentication settings
- Logging configuration
- Tool-specific parameters