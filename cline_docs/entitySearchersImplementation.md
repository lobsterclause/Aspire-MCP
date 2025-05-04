# Universal Search Tool for Aspire MCP Server

The Universal Search tool enables powerful search capabilities across all Aspire entity types from a single unified interface. This guide explains how to use it effectively.

## Overview

The Universal Search tool lets you search across multiple Aspire entity types (Contacts, TimeEntries, Invoices, etc.) with a single query. The search is intelligent and returns ranked results based on relevance to your search term.

## Usage

The UniversalSearch tool accepts the following parameters:

### Required Parameters

- `searchTerm`: The term to search for across all entities

### Optional Parameters

- `entityTypes`: Array of entity types to search (if empty, searches all types)
- `dateRange`: Date range for time-based entities (default: "lastMonth")
- `startDate`/`endDate`: Custom date range (required if dateRange is "custom")
- `maxResults`: Maximum number of results per entity type (default: 100)
- `includeMetadata`: Include detailed metadata in results (default: true)

## Supported Entity Types

The tool can search across the following entity types:

1. TimeEntries - Work hours logged by employees
2. Contacts - Customers, vendors, and employees
3. Divisions - Company departments or divisions
4. Branches - Physical office locations
5. Invoices - Customer billing documents
6. WorkTickets - Service tickets and work orders
7. Opportunities - Sales opportunities
8. Jobs - Projects and ongoing work
9. InventoryItems - Physical inventory and products

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

## Example Queries

Here are some example queries to get you started:

### Basic Search

```json
{
  "searchTerm": "acme corp"
}
```

This will search all entity types for items related to "acme corp".

### Search Specific Entity Types

```json
{
  "searchTerm": "john smith",
  "entityTypes": ["Contacts", "TimeEntries"]
}
```

This will search only contacts and time entries for "john smith".

### Search with Date Range

```json
{
  "searchTerm": "project x",
  "dateRange": "thisMonth",
  "entityTypes": ["TimeEntries", "Jobs", "WorkTickets"]
}
```

This will search time entries, jobs, and work tickets from the current month for "project x".

### Search with Custom Date Range

```json
{
  "searchTerm": "renovation",
  "dateRange": "custom",
  "startDate": "2025-01-01",
  "endDate": "2025-03-31",
  "entityTypes": ["Jobs"]
}
```

This will search jobs from January through March 2025 for "renovation".

### Limit Results

```json
{
  "searchTerm": "maintenance",
  "maxResults": 10,
  "includeMetadata": false
}
```

This will search all entity types for "maintenance", returning up to 10 results per entity type with minimal metadata.

## Response Format

The response is a JSON object with the following structure:

```json
{
  "totalResults": 42,
  "resultsByEntityType": {
    "Contacts": [
      {
        "id": "12345",
        "title": "John Smith",
        "description": "customer: john@example.com",
        "entityType": "Contacts",
        "matchScore": 95,
        "lastModified": "2025-04-15T14:30:00Z",
        "url": "/contacts/12345",
        "data": { ... }
      }
      // More contact results...
    ],
    "TimeEntries": [
      // Time entry results...
    ]
    // Other entity types...
  },
  "searchTime": "2025-04-29T10:15:30Z"
}
```

Each result includes:

- `id`: Unique identifier for the entity
- `title`: Display title for the result
- `description`: Brief description of the entity
- `entityType`: Type of entity
- `matchScore`: Relevance score (0-100)
- `lastModified`: When the entity was last modified (if available)
- `url`: URL to view the entity in Aspire
- `data`: Entity-specific data fields (if includeMetadata is true)

## Integration with AI Assistants

The Universal Search tool is designed to work seamlessly with AI assistants through the Model Context Protocol (MCP). When users ask questions about Aspire data, the AI can:

1. Formulate appropriate search parameters based on the natural language query
2. Call the UniversalSearch tool to retrieve relevant data
3. Process and present the results in a user-friendly format

For example, a user could ask:

"Find all time entries for the downtown office renovation project from the last quarter."

The AI would translate this into a structured search request, specifying the appropriate entity types, date range, and search terms.

## Advanced Search Techniques

### Searching for Specific Fields

The search term will be matched against all relevant fields for each entity type. However, you can target specific fields by using keywords in your search:

- For contacts: Include email domains, phone numbers, or location details
- For time entries: Include employee names or notes content
- For invoices: Include invoice numbers or dollar amounts

### Combining with Other Tools

For more complex scenarios, you can combine the Universal Search tool with other MCP tools:

1. Use UniversalSearch to find relevant entities
2. Use entity-specific tools to get detailed information
3. Use reporting tools to analyze the data

## Troubleshooting

If you're not getting the expected results:

1. **Try broader search terms** - The search looks for exact matches first, then partial matches
2. **Expand your date range** - If time-based entities aren't appearing, they might be outside your date range
3. **Check entity types** - Make sure you're searching the appropriate entity types
4. **Increase maxResults** - Your results might be getting cut off by the result limit