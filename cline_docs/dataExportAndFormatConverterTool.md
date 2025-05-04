# Data Export & Format Converter Tool for Aspire MCP Server

The Data Export & Format Converter tool enables you to export data from your Aspire system in various formats for use in other applications. This guide explains how to use it effectively.

## Overview

The Data Export tool provides a consistent way to extract data from any Aspire entity and convert it to common formats such as CSV, JSON, or Excel. This capability makes it easy to:

- Generate reports for stakeholders
- Import Aspire data into other business systems
- Perform advanced analysis using external tools
- Archive data for compliance or historical purposes
- Share data with team members who don't have Aspire access

## Usage

The ExportData tool accepts the following parameters:

### Required Parameters

- `entityType`: The type of entity to export (TimeEntries, Contacts, etc.)

### Optional Parameters

- `format`: Format of the export (json, csv, excel) - default: csv
- `dateRange`: Date range for time-based entities - default: thisMonth
- `startDate`/`endDate`: Custom date range (required if dateRange is "custom")
- `filters`: Array of filter conditions to apply
- `includeHeaders`: Include column headers in CSV (default: true)
- `delimiter`: Character to use as delimiter in CSV (default: ",")
- `fileName`: Base name for the exported file (default: "aspire_export")

## Supported Entity Types

The tool can export data from all Aspire entity types:

1. TimeEntries - Work hours logged by employees
2. Contacts - Customers, vendors, and employees
3. Divisions - Company departments or divisions
4. Branches - Physical office locations
5. Invoices - Customer billing documents
6. WorkTickets - Service tickets and work orders
7. Opportunities - Sales opportunities
8. Jobs - Projects and ongoing work
9. InventoryItems - Physical inventory and products

## Output Formats

The tool supports the following output formats:

- `json`: Standard JSON format with proper indentation
- `csv`: Comma-separated values (or custom delimiter)
- `excel`: Microsoft Excel XLSX format

## Date Ranges

For time-based entities, you can specify a date range using the following options:

- `today` - Current day only
- `yesterday` - Previous day only
- `thisWeek` - Current calendar week
- `lastWeek` - Previous calendar week
- `thisMonth` - Current calendar month
- `lastMonth` - Previous calendar month
- `thisQuarter` - Current quarter
- `lastQuarter` - Previous quarter
- `thisYear` - Current year
- `lastYear` - Previous year
- `custom` - Specify your own date range using startDate and endDate

## Filtering Data

You can apply filters to export only the data you need:

```json
"filters": [
  {
    "field": "Hours",
    "operator": "gt",
    "value": "8"
  },
  {
    "field": "DivisionName",
    "operator": "contains",
    "value": "Sales"
  }
]
```

Supported operators include:
- `eq`: Equal to
- `neq`: Not equal to
- `gt`: Greater than
- `lt`: Less than
- `gte`: Greater than or equal to
- `lte`: Less than or equal to
- `contains`: Contains the specified text

## Example Queries

Here are some example queries to get you started:

### Basic CSV Export

```json
{
  "entityType": "Contacts",
  "format": "csv"
}
```

This will export all contacts in CSV format with default settings.

### Filtered JSON Export

```json
{
  "entityType": "TimeEntries",
  "format": "json",
  "dateRange": "lastMonth",
  "filters": [
    {
      "field": "DivisionName",
      "operator": "eq",
      "value": "IT Services"
    }
  ]
}
```

This will export time entries from the last month for the IT Services division in JSON format.

### Custom Date Range Excel Export

```json
{
  "entityType": "Invoices",
  "format": "excel",
  "dateRange": "custom",
  "startDate": "2025-01-01",
  "endDate": "2025-03-31",
  "fileName": "Q1_invoices"
}
```

This will export invoices from Q1 2025 in Excel format with the filename "Q1_invoices.xlsx".

### CSV with Custom Delimiter

```json
{
  "entityType": "WorkTickets",
  "format": "csv",
  "delimiter": "|",
  "includeHeaders": true
}
```

This will export work tickets as a pipe-delimited CSV file with column headers.

## Response Format

The response is a JSON object with the following structure:

```json
{
  "fileName": "aspire_export.csv",
  "contentType": "text/csv",
  "fileSize": 42586,
  "data": "base64-encoded-file-data",
  "entityType": "TimeEntries",
  "recordCount": 126,
  "format": "csv",
  "exportedAt": "2025-04-29T15:30:45Z"
}
```

Key fields include:
- `fileName`: Name of the exported file
- `contentType`: MIME type of the file
- `fileSize`: Size in bytes
- `data`: Base64-encoded file data
- `recordCount`: Number of records exported
- `exportedAt`: Timestamp when the export was generated

## Integration with AI Assistants

The Data Export tool is designed to work seamlessly with AI assistants through the Model Context Protocol (MCP). When users ask questions about exporting Aspire data, the AI can:

1. Identify the entity type and format from the natural language query
2. Call the ExportData tool with appropriate parameters
3. Present the exported file for download

For example, a user could ask:

"Export all invoices from Q1 as an Excel file named 'quarterly_invoices'."

The AI would translate this into a structured request for the ExportData tool, specifying the entity type (Invoices), format (excel), date range (custom), start/end dates, and filename.

## Common Use Cases

### Exporting Contact Lists

```json
{
  "entityType": "Contacts",
  "format": "csv",
  "filters": [
    {
      "field": "Type",
      "operator": "eq",
      "value": "customer"
    }
  ],
  "fileName": "customer_contacts"
}
```

This exports a list of all customer contacts as a CSV file.

### Financial Reporting

```json
{
  "entityType": "Invoices",
  "format": "excel",
  "dateRange": "lastMonth",
  "fileName": "monthly_invoice_report"
}
```

This exports last month's invoices as an Excel file for financial reporting.

### Time Entry Analysis

```json
{
  "entityType": "TimeEntries",
  "format": "json",
  "dateRange": "thisYear",
  "filters": [
    {
      "field": "EmployeeName",
      "operator": "eq",
      "value": "Jane Smith"
    }
  ],
  "fileName": "jane_time_entries"
}
```

This exports all of Jane's time entries from the current year for analysis.

## Troubleshooting

If you encounter issues with data exports:

1. **Check entity type spelling** - Entity types are case-sensitive
2. **Verify date formats** - Use YYYY-MM-DD format for custom dates
3. **Test filters individually** - Complex filter combinations may need adjustment
4. **Check field names** - Field names in filters must match exactly
5. **Consider file size limits** - Very large exports may need to be broken into smaller chunks

## Combining with Other Tools

For more powerful workflows, combine the Data Export tool with other MCP tools:

1. Use UniversalSearch to find relevant entities
2. Use EntityRelationshipExplorer to discover related records
3. Use ExportData to export the combined dataset
4. Use BuildCustomReport to create visualizations from the data