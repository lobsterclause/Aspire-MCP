import asyncio
import json
import sys
from aspire_api import AspireAPI

# Define tool schemas
TOOL_DEFINITIONS = {
    "get_api_version": {
        "description": "Fetches the Aspire API version.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    },
    "list_branches": {
        "description": "Lists all branches from the Aspire API.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    },
    "list_divisions": {
        "description": "Lists all divisions from the Aspire API.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    },
    "list_contacts": {
        "description": "Lists all contacts from the Aspire API with optional OData filtering.",
        "input_schema": {
            "type": "object",
            "properties": {
                "filter": {
                    "type": "string",
                    "description": "OData filter string (e.g., \"BranchName eq 'California'\")"
                }
            }
        }
    },
    "list_properties": {
        "description": "Lists all properties from the Aspire API.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    }
}

class MCPServer:
    def __init__(self):
        self._tools = {}
        self.aspire_api = AspireAPI() # Create an instance of AspireAPI
        self._register_tools()

    def register_tool(self, tool_name, handler):
        self._tools[tool_name] = handler

    def _register_tools(self):
        # Register implemented tool handlers
        self.register_tool("get_api_version", self._handle_get_api_version)
        self.register_tool("list_branches", self._handle_list_branches)
        self.register_tool("list_contacts", self._handle_list_contacts)
        self.register_tool("list_properties", self._handle_list_properties)
        # Register other tools here as they are implemented

    async def _handle_get_api_version(self, arguments):
        """Handler for the get_api_version tool."""
        # Arguments are not used for this tool, but the handler signature requires it
        version_data = await self.aspire_api.get_api_version()
        if version_data:
            return {"status": "success", "data": version_data}
        else:
            # Return an error result if the API call failed
            return {"status": "error", "message": "Failed to fetch API version from Aspire API."}

    async def _handle_list_branches(self, arguments):
        """Handler for the list_branches tool."""
        # Arguments are not used for this tool
        branches_data = await self.aspire_api.get_branches()
        if branches_data is not None:
            return {"status": "success", "data": branches_data}
        else:
            # Return an error result if the API call failed
            return {"status": "error", "message": "Failed to fetch branches from Aspire API."}

    async def _handle_list_divisions(self, arguments):
        """Handler for the list_divisions tool."""
        # Arguments are not used for this tool
        divisions_data = await self.aspire_api.get_divisions()
        if divisions_data is not None:
            return {"status": "success", "data": divisions_data}
        else:
            # Return an error result if the API call failed
            return {"status": "error", "message": "Failed to fetch divisions from Aspire API."}

    async def _handle_list_contacts(self, arguments):
        """Handler for the list_contacts tool."""
        # Extract the optional filter parameter
        filter_query = arguments.get("filter")
        
        contacts_data = await self.aspire_api.get_contacts(filter_query)
        if contacts_data is not None:
            return {"status": "success", "data": contacts_data}
        else:
            # Return an error result if the API call failed
            return {"status": "error", "message": "Failed to fetch contacts from Aspire API."}

    async def _handle_list_properties(self, arguments):
        """Handler for the list_properties tool."""
        # Arguments are not used for this tool
        properties_data = await self.aspire_api.get_properties()
        if properties_data is not None:
            return {"status": "success", "data": properties_data}
        else:
            # Return an error result if the API call failed
            return {"status": "error", "message": "Failed to fetch properties from Aspire API."}

    async def _handle_message(self, message):
        try:
            msg = json.loads(message)
            msg_type = msg.get("type")
            msg_id = msg.get("id", "unknown")

            if msg_type == "tool_call":
                tool_name = msg.get("tool_name")
                arguments = msg.get("arguments", {})

                if tool_name in self._tools:
                    handler = self._tools[tool_name]
                    try:
                        result = await handler(arguments)
                        response = {
                            "type": "response",
                            "id": msg_id,
                            "result": result
                        }
                        await self._send_message(json.dumps(response))
                    except Exception as e:
                        error_response = {
                            "type": "error",
                            "id": msg_id,
                            "error": {"code": 500, "message": f"Tool execution error: {e}"}
                        }
                        await self._send_message(json.dumps(error_response))
                else:
                    error_response = {
                        "type": "error",
                        "id": msg_id,
                        "error": {"code": 404, "message": f"Tool '{tool_name}' not found."}
                    }
                    await self._send_message(json.dumps(error_response))
            else:
                # Handle other message types or unknown types
                response = {
                    "type": "response",
                    "id": msg_id,
                    "result": {"status": "ignored", "message": f"Unknown message type: {msg_type}"}
                }
                await self._send_message(json.dumps(response))

        except json.JSONDecodeError:
            error_response = {
                "type": "error",
                "id": "unknown",
                "error": {"code": 400, "message": "Invalid JSON received."}
            }
            await self._send_message(json.dumps(error_response))
        except Exception as e:
            error_response = {
                "type": "error",
                "id": msg.get("id", "unknown") if 'msg' in locals() else "unknown",
                "error": {"code": 500, "message": f"Internal server error during message handling: {e}"}
            }
            await self._send_message(json.dumps(error_response))

    async def _send_message(self, message):
        sys.stdout.write(message + '\n')
        sys.stdout.flush()

    async def start_stdio(self):
        print("MCP Server starting in stdio mode...")
        loop = asyncio.get_event_loop()
        while True:
            line = await loop.run_in_executor(None, sys.stdin.readline)
            if not line:
                print("Stdin closed, shutting down.")
                break
            await self._handle_message(line.strip())

    async def start(self, mode="stdio"):
        if mode == "stdio":
            await self.start_stdio()
        else:
            print(f"Unsupported mode: {mode}")

if __name__ == "__main__":
    # This part is for basic testing, main application logic will be in main.py
    async def test_server():
        server = MCPServer()
        # Simulate receiving a message
        await server._handle_message('{"id": "123", "type": "tool_call", "tool_name": "test_tool", "arguments": {}}')
        await server._handle_message('invalid json')

    import asyncio
    asyncio.run(test_server())