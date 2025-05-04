# AspireMCP Server Test Scripts

This directory contains test scripts to verify the functionality of the AspireMCP server once it's running.

## Overview

The main test script (`test-mcp-server.js`) performs the following tests:

1. **Server Connection Test**: Verifies that the MCP server is running and accessible
2. **Basic Tool Test**: Tests the ListPayments tool with minimal parameters
3. **Advanced Query Test**: Tests the ListPayments tool with OData query parameters and relationship expansion

## Prerequisites

- [Node.js](https://nodejs.org/) (v12 or higher)
- npm or yarn package manager

## Setup

Install the required dependencies:

```bash
cd TestScripts
npm install
```

## Running the Tests

### Step 1: Start the AspireMCP Server

First, ensure the AspireMCP server is running. From the project root directory:

```bash
cd AspireAPI
dotnet run
```

By default, the server should start listening on `http://localhost:5000/mcp`.

### Step 2: Run the Test Script

Once the server is running, open a new terminal and run the test script:

```bash
cd TestScripts
node test-mcp-server.js
```

### Command-Line Options

The test script accepts the following command-line options:

- `--server=URL`: Specify the MCP server URL (default: `http://localhost:5000/mcp`)
- `--tool=TOOL_NAME`: Specify which tool to test (default: `ListPayments`)

Example:

```bash
node test-mcp-server.js --server=http://localhost:5050/mcp
```

## Understanding the Output

The script provides color-coded output to easily identify:

- ✓ Green text indicates successful operations
- ✗ Red text indicates failed operations
- ⚠ Yellow text indicates warnings or information
- Gray text shows additional details

A test summary is shown at the end, indicating which tests passed or failed.

## Troubleshooting

If the tests fail, check the following:

1. **Connection Failures**:
   - Ensure the AspireMCP server is running
   - Verify the server URL is correct
   - Check if any firewall is blocking the connection

2. **Tool Execution Failures**:
   - Check the AspireMCP server logs for errors
   - Verify that the ListPayments tool is properly registered in the server
   - Ensure the tool parameters are valid

3. **Advanced Query Failures**:
   - Verify that the OData query syntax is correct
   - Check if the specified fields for filtering exist
   - Ensure the expand paths are valid

## Error Codes

The script exits with the following codes:

- `0`: All tests passed successfully
- `1`: One or more tests failed

This allows the script to be used in CI/CD pipelines to automatically verify the MCP server functionality.