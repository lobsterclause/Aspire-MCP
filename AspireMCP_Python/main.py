import asyncio
import argparse
from mcp_server import MCPServer
from aspire_api import AspireAPI
from config import MCP_MODE

async def main():
    parser = argparse.ArgumentParser(description="Aspire MCP Python Server")
    parser.add_argument("--mode", help="MCP connection mode (stdio, sse)", default=MCP_MODE)
    # Add other arguments for configuration if needed in the future

    args = parser.parse_args()

    # Initialize Aspire API (will use environment variables or placeholders from config.py)
    aspire_api = AspireAPI()

    # Initialize MCP Server
    # Trigger authentication on startup to verify logging
    await aspire_api.get_api_version()
    server = MCPServer()

    # TODO: Register tools here in future tasks

    print(f"Starting MCP server in {args.mode} mode...")
    await server.start(mode=args.mode)

if __name__ == "__main__":
    asyncio.run(main())