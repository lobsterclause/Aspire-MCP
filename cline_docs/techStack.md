## Tech Stack

## Core Technologies
- **.NET 8.0:** The primary development framework.
- **.NET Aspire:** Used for orchestration and setting up the MCP server environment.
- **C#:** The main programming language.

## Protocols and APIs
- **Model Context Protocol (MCP):** The protocol for AI model interaction.
- **Aspire Cloud API:** The external API the server will interact with.

## Libraries and SDKs
- **Microsoft's MCP C# SDK:** Used for implementing the MCP server.

## Architecture Decisions
- **Two-Project Structure:** Separating the AppHost orchestration from the AspireAPI server implementation for clarity and modularity.
- **Environment Variable Configuration:** Using environment variables for sensitive information like API credentials and base URL.