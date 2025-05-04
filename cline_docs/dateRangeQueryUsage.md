# Aspire MCP Server

This project implements a Model Context Protocol (MCP) server for Aspire Cloud API, allowing AI models to interact with your Aspire data through standardized tools.

## What is Model Context Protocol?

The Model Context Protocol (MCP) is an open protocol designed to standardize how applications provide context to Large Language Models (LLMs). It enables seamless integration between AI models and external data sources and tools. 

In simpler terms, MCP provides a standard way for AI assistants like GitHub Copilot or Claude to connect with and use your business systems.

## Project Structure

This solution consists of:

1. **AppHost** - A .NET Aspire orchestration project that sets up the MCP server
2. **AspireAPI** - The MCP server implementation that connects to Aspire Cloud API

## Features

The MCP server exposes the following tools to AI models:

- **GetTimeEntryReport** - Generates time entry reports with filters for client, division, and date range
- **ListContacts** - Lists contacts (customers, vendors, employees) with optional search
- **ListDivisions** - Lists divisions with optional search

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Aspire Cloud API credentials

### Setup

1. Clone this repository
2. Configure environment variables:

```bash
export ASPIRE__BASE_URL=https://cloud-api.youraspire.com
export ASPIRE__USERNAME=api.integration@yourdomain.com
export ASPIRE__PASSWORD=your-password-here
export ASPIRE__COMPANYKEY=ACME-LAN
```

3. Run the application:

```bash
cd AspireAppHost
dotnet run
```

### Testing with MCP Inspector

The Aspire configuration automatically includes the MCP Inspector, which allows you to test your MCP server.

1. Once running, open http://127.0.0.1:6274 in your browser
2. Click "Connect"
3. Test the available tools:

```json
// Example: Get time entry report
{
  "clientName": "Acme Corp",
  "divisionName": "IT Services",
  "dateRange": "lastWeek"
}

// Example: List contacts
{
  "type": "customer",
  "search": "Acme"
}
```

## Integration with AI Tools

This MCP server can be used with:

- GitHub Copilot X
- Microsoft Visual Studio with Copilot
- Any LLM tool that supports the Model Context Protocol

## How It Works

1. The MCP server exposes a standardized interface to AI models
2. When the AI model needs data from Aspire, it calls the appropriate tool
3. The MCP server handles authentication, queries the Aspire API, and returns formatted results
4. The AI model can then use this data to respond to user requests

## Next Steps

- Add more tools for other Aspire API endpoints
- Implement caching for improved performance
- Add more sophisticated report generation capabilities

## Resources

- [Model Context Protocol Documentation](https://github.com/modelcontextprotocol/protocol)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Microsoft's MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)