#!/bin/bash

echo "==================================================="
echo "      AspireMCP Server - Test Script"
echo "==================================================="
echo ""

SERVER_URL="http://localhost:5000/mcp"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_SCRIPTS_DIR="$SCRIPT_DIR/TestScripts"

echo "Step 1: Checking if AspireMCP server is running..."
# Check if server is running
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$SERVER_URL/capabilities" || echo "000")

if [ "$STATUS" != "200" ]; then
    echo "Error: AspireMCP server does not appear to be running at $SERVER_URL"
    echo "Status code: $STATUS"
    echo "Please start the server using ./run-aspire-mcp.sh before running tests."
    exit 1
fi
echo "Server is running. Status code: $STATUS"
echo ""

echo "Step 2: Setting up test environment..."
cd "$TEST_SCRIPTS_DIR" || { echo "Error: TestScripts directory not found"; exit 1; }

# Check if Node.js dependencies are installed
if [ ! -d "node_modules" ]; then
    echo "Installing Node.js dependencies..."
    npm install
    if [ $? -ne 0 ]; then
        echo "Error: Failed to install Node.js dependencies"
        exit 1
    fi
    echo "Dependencies installed successfully."
else
    echo "Node.js dependencies already installed."
fi
echo ""

echo "Step 3: Running tests against $SERVER_URL..."
node test-mcp-server.js --server="$SERVER_URL"
TEST_RESULT=$?

echo ""
echo "==================================================="
echo "Test Summary"
echo "==================================================="
if [ $TEST_RESULT -eq 0 ]; then
    echo "All tests passed successfully!"
else
    echo "Some tests failed. See the output above for details."
fi

exit $TEST_RESULT