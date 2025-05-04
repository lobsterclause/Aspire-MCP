# MCP Inspector Testing

This document details the testing procedures for the Aspire MCP server tools using the MCP Inspector.

## Setup

1.  Ensure the Aspire MCP server is running.
2.  Launch the MCP Inspector and connect it to the running Aspire MCP server.

## Testing Procedures

For each tool listed below, use the MCP Inspector to call the tool with various valid and invalid inputs. Observe the output to ensure it matches the expected behavior and data structure.

### GetTimeEntryReport Tool

*   **Description:** Get a time entry report from Aspire Cloud API.
*   **Input Schema:**
    ```json
    {
      "type": "object",
      "properties": {
        "clientName": { "type": "string", "description": "Name of the client" },
        "divisionName": { "type": "string", "description": "Name of the division (optional)" },
        "dateRange": {
          "type": "string",
          "description": "Date range for the report",
          "enum": ["lastWeek", "thisWeek", "thisMonth", "lastMonth", "lastQuarter", "lastYear", "yearToDate", "custom"]
        },
        "startDate": { "type": "string", "description": "Start date (required if dateRange is 'custom')" },
        "endDate": { "type": "string", "description": "End date (required if dateRange is 'custom')" },
        "groupBy": {
          "type": "string",
          "description": "Field to group the report by (optional)",
          "enum": ["employee", "client", "division", "branch", "date"]
        },
        "includeMetrics": {
          "type": "array",
          "items": { "type": "string" },
          "description": "List of metrics to include (optional, e.g., 'totalHours', 'billableHours', 'utilization')"
        }
      },
      "required": ["dateRange"]
    }
    ```
*   **Testing Scenarios:**
    *   Test with various `dateRange` shortcuts (thisWeek, lastWeek, thisMonth, lastMonth, lastQuarter, lastYear, yearToDate).
    *   Test with a `custom` date range, providing `startDate` and `endDate`.
    *   Test with a `custom` date range where `startDate` or `endDate` is missing.
    *   Test with different `groupBy` options (employee, client, division, branch, date).
    *   Test with `clientName` filter.
    *   Test with `divisionName` filter.
    *   Test with `includeMetrics` (e.g., ["totalHours"]).
    *   Test combinations of date ranges, grouping, and filters.
    *   Test with invalid input values (e.g., invalid date format, invalid `dateRange`, `groupBy`, or `includeMetrics` values).

### ListContacts Tool

*   **Description:** List contacts from Aspire Cloud API.
*   **Input Schema:**
    ```json
    {
      "type": "object",
      "properties": {
        "type": {
          "type": "string",
          "description": "Type of contacts to list",
          "enum": ["customer", "vendor", "employee", "all"]
        },
        "search": { "type": "string", "description": "Search term (optional)" }
      },
      "required": ["type"]
    }
    ```
*   **Testing Scenarios:**
    *   Test with each `type` (customer, vendor, employee, all).
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with invalid `type` values.

### ListDivisions Tool

*   **Description:** List divisions from Aspire Cloud API.
*   **Input Schema:**
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.

### ListBranches Tool

*   **Description:** List branches from Aspire Cloud API.
*   **Input Schema:** (Inferring based on ListDivisions)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.

### ListOpportunities Tool

*   **Description:** List opportunities from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "status": { "type": "string", "description": "Filter by status (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with a `status` filter.
    *   Test with combinations of `search` and `status`.

### ListInvoices Tool

*   **Description:** List invoices from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "status": { "type": "string", "description": "Filter by status (optional)" },
        "dateRange": {
          "type": "string",
          "description": "Date range for invoices (optional)",
          "enum": ["lastWeek", "thisWeek", "thisMonth", "lastMonth", "lastQuarter", "lastYear", "yearToDate", "custom"]
        },
        "startDate": { "type": "string", "description": "Start date (required if dateRange is 'custom')" },
        "endDate": { "type": "string", "description": "End date (required if dateRange is 'custom')" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with a `status` filter.
    *   Test with various `dateRange` options.
    *   Test combinations of inputs.

### ListWorkTickets Tool

*   **Description:** List work tickets from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "status": { "type": "string", "description": "Filter by status (optional)" },
        "dateRange": {
          "type": "string",
          "description": "Date range for work tickets (optional)",
          "enum": ["lastWeek", "thisWeek", "thisMonth", "lastMonth", "lastQuarter", "lastYear", "yearToDate", "custom"]
        },
        "startDate": { "type": "string", "description": "Start date (required if dateRange is 'custom')" },
        "endDate": { "type": "string", "description": "End date (required if dateRange is 'custom')" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with a `status` filter.
    *   Test with various `dateRange` options.
    *   Test combinations of inputs.

### ListJobs Tool

*   **Description:** List jobs from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "status": { "type": "string", "description": "Filter by status (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with a `status` filter.
    *   Test with combinations of `search` and `status`.

### GetScheduleBoard Tool

*   **Description:** Get schedule board data from Aspire Cloud API.
*   **Input Schema:** (Inferring based on potential parameters)
    ```json
    {
      "type": "object",
      "properties": {
        "date": { "type": "string", "description": "Date for the schedule board (YYYY-MM-DD, optional, defaults to today)" },
        "divisionName": { "type": "string", "description": "Filter by division (optional)" },
        "branchName": { "type": "string", "description": "Filter by branch (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with no input (should default to today).
    *   Test with a specific `date`.
    *   Test with `divisionName` filter.
    *   Test with `branchName` filter.
    *   Test with combinations of inputs.
    *   Test with invalid date format.

### ListInventoryItems Tool

*   **Description:** List inventory items from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "category": { "type": "string", "description": "Filter by category (optional)" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with a `category` filter.
    *   Test with combinations of `search` and `category`.

### ListPurchaseReceipts Tool

*   **Description:** List purchase receipts from Aspire Cloud API.
*   **Input Schema:** (Inferring based on common list patterns)
    ```json
    {
      "type": "object",
      "properties": {
        "search": { "type": "string", "description": "Search term (optional)" },
        "dateRange": {
          "type": "string",
          "description": "Date range for receipts (optional)",
          "enum": ["lastWeek", "thisWeek", "thisMonth", "lastMonth", "lastQuarter", "lastYear", "yearToDate", "custom"]
        },
        "startDate": { "type": "string", "description": "Start date (required if dateRange is 'custom')" },
        "endDate": { "type": "string", "description": "End date (required if dateRange is 'custom')" }
      }
    }
    ```
*   **Testing Scenarios:**
    *   Test with a `search` term.
    *   Test with an empty `search` term.
    *   Test with various `dateRange` options.
    *   Test combinations of inputs.

### Custom Report Builder Tools (BuildCustomReport, BuildTimeEntryAnalysisReport, BuildClientProfitabilityReport)

*   **Note:** These tools appear to be commented out in `AspireMcpServer.cs`. Testing should be performed once they are uncommented and fully implemented.
*   **Description:** Tools for building custom reports based on flexible definitions or predefined analysis types.
*   **Input Schema:** Refer to the `ReportDefinition`, `DataSource`, `CalculationDefinition`, `AggregationDefinition`, `FilterCondition`, and `SortDefinition` models in the codebase for the expected structure of the `reportDefinition` argument for `BuildCustomReport`. For `BuildTimeEntryAnalysisReport` and `BuildClientProfitabilityReport`, refer to their respective handler implementations (once uncommented) for expected parameters.
*   **Testing Scenarios:**
    *   Once implemented, test `BuildCustomReport` with various `ReportDefinition` configurations, including different data sources, filters, calculations, groupings, and aggregations.
    *   Once implemented, test `BuildTimeEntryAnalysisReport` and `BuildClientProfitabilityReport` with various parameters to ensure they correctly generate the underlying report definitions and produce expected results.

### TrackWorkflowStatus Tool

*   **Note:** This tool appears to be commented out in `AspireMcpServer.cs`. Testing should be performed once it is uncommented and fully implemented.
*   **Description:** Tool for tracking entities through workflow stages.
*   **Input Schema:** Refer to the `WorkflowParameters` model and the handler implementation (once uncommented) for expected parameters.
*   **Testing Scenarios:**
    *   Once implemented, test with various entity types and identifiers to track their workflow status.
    *   Test with invalid entity types or identifiers.

## Summary of Results

[Record the overall testing outcome here after performing the tests using the MCP Inspector]

## Conclusion

[Provide a brief conclusion on the readiness of the tools for use with the MCP Inspector after performing the tests]