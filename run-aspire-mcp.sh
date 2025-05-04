#!/bin/bash

echo "==================================================="
echo "     AspireMCP Server - Build and Run Script"
echo "==================================================="
echo ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ASPIRE_API_DIR="$SCRIPT_DIR/AspireAPI"

echo "Step 1: Building AspireAPI project..."
cd "$ASPIRE_API_DIR" || { echo "Error: AspireAPI directory not found"; exit 1; }
dotnet build
if [ $? -ne 0 ]; then
    echo "Error: Build failed"
    exit 1
fi
echo "Build completed successfully."
echo ""

echo "Step 2: Starting AspireMCP server..."
echo "Server URL: http://localhost:5000/mcp"
echo ""
echo "Press Ctrl+C to stop the server."
echo "To test the server, open another terminal and run ./test-server.sh"
echo ""

cd "$ASPIRE_API_DIR" || { echo "Error: AspireAPI directory not found"; exit 1; }
dotnet run

echo "Server stopped."
exit 0