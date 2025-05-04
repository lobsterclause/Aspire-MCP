/**
 * AspireMCP Server Test Script
 * This script connects to the AspireMCP server and tests the ListPayments tool
 */

const fetch = require('node-fetch');
const chalk = require('chalk');

// Configuration
const config = {
  // Default to localhost, but can be changed via command-line args
  serverUrl: process.env.MCP_SERVER_URL || 'http://localhost:5000/mcp',
  tool: 'ListPayments'
};

// Process command-line arguments
for (let i = 2; i < process.argv.length; i++) {
  const arg = process.argv[i];
  if (arg.startsWith('--server=')) {
    config.serverUrl = arg.substring('--server='.length);
  } else if (arg.startsWith('--tool=')) {
    config.tool = arg.substring('--tool='.length);
  }
}

/**
 * Test the MCP server connection
 */
async function testServerConnection() {
  try {
    console.log(chalk.blue('╔════════════════════════════════════════════════╗'));
    console.log(chalk.blue('║        AspireMCP Server Connection Test        ║'));
    console.log(chalk.blue('╚════════════════════════════════════════════════╝'));
    console.log(chalk.yellow(`Attempting to connect to: ${config.serverUrl}`));

    const response = await fetch(`${config.serverUrl}/capabilities`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error(`Failed to connect to server: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();
    console.log(chalk.green('✓ Successfully connected to MCP server'));
    console.log(chalk.gray('Server capabilities:'), data);
    return true;
  } catch (error) {
    console.error(chalk.red('✗ Connection failed:'), error.message);
    console.error(chalk.yellow('Make sure the AspireMCP server is running.'));
    return false;
  }
}

/**
 * Test the ListPayments tool
 */
async function testListPaymentsTool() {
  try {
    console.log(chalk.blue('\n╔════════════════════════════════════════════════╗'));
    console.log(chalk.blue('║           Testing ListPayments Tool            ║'));
    console.log(chalk.blue('╚════════════════════════════════════════════════╝'));

    // Prepare the tool request
    const toolRequest = {
      name: 'ListPayments',
      arguments: {
        // Start with minimal parameters for the first test
        pageSize: 10,
        pageNumber: 1,
        useCache: false
      }
    };

    console.log(chalk.yellow('Sending request with parameters:'));
    console.log(chalk.gray(JSON.stringify(toolRequest.arguments, null, 2)));

    const response = await fetch(`${config.serverUrl}/tools/call`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(toolRequest)
    });

    // Check if the response is successful
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Tool execution failed: ${response.status} ${response.statusText}\n${errorText}`);
    }

    // Parse the response
    const data = await response.json();
    console.log(chalk.green('✓ Tool execution successful'));
    
    // Show a summary of the results
    if (data && data.content && data.content[0] && data.content[0].json) {
      const result = JSON.parse(data.content[0].json);
      console.log(chalk.green(`Retrieved ${result.data?.length || 0} payments`));
      
      // Log first few results for verification
      if (result.data && result.data.length > 0) {
        console.log(chalk.yellow('\nSample results:'));
        result.data.slice(0, 3).forEach((payment, index) => {
          console.log(chalk.cyan(`Payment ${index + 1}:`), 
            `ID: ${payment.id || 'N/A'}, ` +
            `Amount: ${payment.amount || 'N/A'}, ` +
            `Date: ${payment.date || 'N/A'}, ` +
            `Status: ${payment.status || 'N/A'}`);
        });
      }
    } else {
      console.log(chalk.yellow('Received response, but result format was unexpected'));
      console.log(chalk.gray('Raw response:'), data);
    }
    
    return true;
  } catch (error) {
    console.error(chalk.red('✗ Tool execution failed:'), error.message);
    return false;
  }
}

/**
 * Test with OData query parameters
 */
async function testAdvancedQuery() {
  try {
    console.log(chalk.blue('\n╔════════════════════════════════════════════════╗'));
    console.log(chalk.blue('║        Testing Advanced Query Parameters        ║'));
    console.log(chalk.blue('╚════════════════════════════════════════════════╝'));

    // Prepare the tool request with OData query
    const toolRequest = {
      name: 'ListPayments',
      arguments: {
        query: "$filter=amount gt 100 and date ge 2023-01-01",
        expand: "invoice,contact",
        pageSize: 5,
        pageNumber: 1,
        useCache: false
      }
    };

    console.log(chalk.yellow('Sending request with OData query:'));
    console.log(chalk.gray(JSON.stringify(toolRequest.arguments, null, 2)));

    const response = await fetch(`${config.serverUrl}/tools/call`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(toolRequest)
    });

    // Check if the response is successful
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Tool execution failed: ${response.status} ${response.statusText}\n${errorText}`);
    }

    // Parse the response
    const data = await response.json();
    console.log(chalk.green('✓ Advanced query execution successful'));
    
    // Log results summary
    if (data && data.content && data.content[0] && data.content[0].json) {
      const result = JSON.parse(data.content[0].json);
      console.log(chalk.green(`Retrieved ${result.data?.length || 0} filtered payments`));
      
      // Check if expanded data is present
      const hasExpandedData = result.data?.some(p => p.invoice || p.contact);
      if (hasExpandedData) {
        console.log(chalk.green('✓ Expanded relationship data retrieved successfully'));
      } else {
        console.log(chalk.yellow('⚠ Expanded data not found in the response'));
      }
    } else {
      console.log(chalk.yellow('Received response, but result format was unexpected'));
      console.log(chalk.gray('Raw response:'), data);
    }
    
    return true;
  } catch (error) {
    console.error(chalk.red('✗ Advanced query execution failed:'), error.message);
    return false;
  }
}

/**
 * Run all tests
 */
async function runTests() {
  console.log(chalk.blue('╔════════════════════════════════════════════════╗'));
  console.log(chalk.blue('║             AspireMCP Server Tester            ║'));
  console.log(chalk.blue('╚════════════════════════════════════════════════╝'));
  console.log(chalk.yellow(`Server URL: ${config.serverUrl}`));
  console.log(chalk.yellow(`Target Tool: ${config.tool}`));
  
  let connectionSuccess = false;
  let testSuccess = false;
  let advancedQuerySuccess = false;
  
  try {
    // Test 1: Server Connection
    connectionSuccess = await testServerConnection();
    
    if (connectionSuccess) {
      // Test 2: Basic Tool Execution
      testSuccess = await testListPaymentsTool();
      
      // Test 3: Advanced Query (only if basic test passed)
      if (testSuccess) {
        advancedQuerySuccess = await testAdvancedQuery();
      }
    }
    
    // Summary
    console.log(chalk.blue('\n╔════════════════════════════════════════════════╗'));
    console.log(chalk.blue('║                   Test Summary                 ║'));
    console.log(chalk.blue('╚════════════════════════════════════════════════╝'));
    console.log(chalk.yellow('Server Connection:  ') + (connectionSuccess ? chalk.green('✓ PASSED') : chalk.red('✗ FAILED')));
    console.log(chalk.yellow('Basic Tool Test:    ') + (testSuccess ? chalk.green('✓ PASSED') : chalk.red('✗ FAILED')));
    console.log(chalk.yellow('Advanced Query:     ') + (advancedQuerySuccess ? chalk.green('✓ PASSED') : (testSuccess ? chalk.red('✗ FAILED') : chalk.gray('⚠ SKIPPED'))));
    
    // Overall result
    const allPassed = connectionSuccess && testSuccess && advancedQuerySuccess;
    console.log(chalk.yellow('\nOverall Test Result: ') + (allPassed ? chalk.green('✓ ALL TESTS PASSED') : chalk.red('✗ SOME TESTS FAILED')));
    
    // Exit with appropriate code
    process.exit(allPassed ? 0 : 1);
  } catch (error) {
    console.error(chalk.red('\nUnexpected error during test execution:'), error);
    process.exit(1);
  }
}

// Start the tests
runTests();