# Entity Relationship Explorer Tool for Aspire MCP Server

The Entity Relationship Explorer tool enables you to discover and navigate the connections between different entities in your Aspire system. This guide explains how to use it effectively.

## Overview

The Entity Relationship Explorer provides a 360-degree view of any entity in your Aspire system, showing its relationships to other entities. It helps you answer questions like:

- What invoices are associated with this customer?
- What employees have worked on this job?
- What time entries are related to this division?
- What parts were used for this work ticket?

## Usage

The EntityRelationshipExplorer tool accepts the following parameters:

### Required Parameters

- `entityType`: The type of entity to explore (TimeEntries, Contacts, etc.)
- `entityId`: The ID of the entity to explore

### Optional Parameters

- `relationshipTypes`: Types of relationships to explore (parent, child, related)
- `maxDepth`: Maximum depth of relationships to explore (default: 1)
- `includeDetails`: Include detailed entity properties (default: true)

## Supported Entity Types

The tool can explore relationships for the following entity types:

1. TimeEntries - Work hours logged by employees
2. Contacts - Customers, vendors, and employees
3. Divisions - Company departments or divisions
4. Branches - Physical office locations
5. Invoices - Customer billing documents
6. WorkTickets - Service tickets and work orders
7. Opportunities - Sales opportunities
8. Jobs - Projects and ongoing work
9. InventoryItems - Physical inventory and products

## Relationship Types

The tool supports the following relationship types:

- `parent`: Entities that contain or own the current entity
- `child`: Entities that are contained by or belong to the current entity
- `related`: Entities that are associated with the current entity

## Example Queries

Here are some example queries to get you started:

### Basic Exploration

```json
{
  "entityType": "Contacts",
  "entityId": "c-12345"
}
```

This will explore relationships for the specified contact, showing immediate parent and child relationships.

### Deeper Exploration

```json
{
  "entityType": "Jobs",
  "entityId": "j-67890",
  "maxDepth": 2
}
```

This will explore relationships for the specified job to a depth of 2 levels, showing relationships of related entities as well.

### Specific Relationship Types

```json
{
  "entityType": "TimeEntries",
  "entityId": "te-54321",
  "relationshipTypes": ["parent"]
}
```

This will only show parent relationships for the specified time entry.

### Minimal Details

```json
{
  "entityType": "Divisions",
  "entityId": "d-202",
  "includeDetails": false
}
```

This will explore relationships for the specified division without including detailed properties.

## Response Format

The response is a JSON object with the following structure:

```json
{
  "entityType": "Contacts",
  "entityId": "c-12345",
  "entityName": "Acme Corporation",
  "entityDetails": {
    "Name": "Acme Corporation",
    "Type": "customer",
    "Email": "info@acmecorp.com",
    "Phone": "555-123-4567",
    "Address": "123 Main St",
    "City": "Metropolis",
    "State": "NY",
    "ZipCode": "10001"
  },
  "relationships": [
    {
      "relationshipType": "child",
      "relatedEntityType": "Invoices",
      "relatedEntityId": "i-54321",
      "relatedEntityName": "Invoice #INV-2025-042",
      "relatedEntityDetails": {
        "Number": "INV-2025-042",
        "Date": "2025-04-15T10:30:00",
        "DueDate": "2025-05-15T00:00:00",
        "Amount": 15750.00,
        "Status": "Open"
      },
      "nestedRelationships": []
    },
    {
      "relationshipType": "child",
      "relatedEntityType": "Jobs",
      "relatedEntityId": "j-67890",
      "relatedEntityName": "Downtown Office Renovation",
      "relatedEntityDetails": {
        "Name": "Downtown Office Renovation",
        "Status": "In Progress",
        "StartDate": "2025-01-15T00:00:00",
        "EndDate": "2025-03-20T00:00:00"
      },
      "nestedRelationships": [
        {
          "relationshipType": "child",
          "relatedEntityType": "WorkTickets",
          "relatedEntityId": "wt-12345",
          "relatedEntityName": "HVAC Installation",
          "relatedEntityDetails": {
            "JobId": "j-67890",
            "JobName": "Downtown Office Renovation",
            "DivisionId": "d-202",
            "DivisionName": "Installation Services",
            "IsDeleted": false
          }
        }
      ]
    }
  ],
  "explorationTime": "2025-04-29T15:30:45Z"
}
```

The response includes:

- Basic information about the entity being explored
- Detailed properties of the entity (if includeDetails is true)
- A list of relationships to other entities
- Each relationship includes:
  - The type of relationship
  - The type, ID, and name of the related entity
  - Detailed properties of the related entity (if includeDetails is true)
  - Nested relationships (if maxDepth > 1)

## Integration with AI Assistants

The Entity Relationship Explorer tool is designed to work seamlessly with AI assistants through the Model Context Protocol (MCP). When users ask questions about relationships between entities, the AI can:

1. Identify the entity type and ID from the natural language query
2. Call the EntityRelationshipExplorer tool to retrieve relationship data
3. Process and present the results in a user-friendly format

For example, a user could ask:

"Show me all invoices for Acme Corporation in the last quarter."

The AI would identify that this is a relationship question about a Contact entity, call the EntityRelationshipExplorer tool with appropriate parameters, and present the invoice relationships.

## Common Use Cases

### Customer 360 View

```json
{
  "entityType": "Contacts",
  "entityId": "c-12345",
  "maxDepth": 2
}
```

This provides a complete view of a customer, including:
- All invoices issued to the customer
- All jobs performed for the customer
- All opportunities associated with the customer
- Time entries billed to the customer

### Job Analysis

```json
{
  "entityType": "Jobs",
  "entityId": "j-67890",
  "maxDepth": 2
}
```

This provides a complete view of a job, including:
- The customer the job is for
- Work tickets associated with the job
- Time entries logged against the job
- Employees who worked on the job
- Inventory items used for the job

### Employee Activity

```json
{
  "entityType": "Contacts",
  "entityId": "e-54321",
  "relationshipTypes": ["child"]
}
```

This shows all activity for an employee, including:
- Time entries logged by the employee
- Jobs the employee has worked on
- Work tickets assigned to the employee

## Advanced Usage

### Combining with Other Tools

For more complex scenarios, you can combine the Entity Relationship Explorer tool with other MCP tools:

1. Use UniversalSearch to find relevant entities
2. Use EntityRelationshipExplorer to discover relationships
3. Use reporting tools to analyze the data

### Relationship Path Discovery

By setting a higher maxDepth, you can discover indirect relationships between entities:

```json
{
  "entityType": "Contacts",
  "entityId": "c-12345",
  "maxDepth": 3
}
```

This might reveal that a customer is indirectly connected to another customer through a shared job or employee.

## Troubleshooting

If you're not getting the expected relationships:

1. **Check the entity type and ID** - Make sure you're using the correct entity type and ID
2. **Increase maxDepth** - Some relationships might be more than one level deep
3. **Check relationship types** - Make sure you're including the appropriate relationship types
4. **Include details** - Set includeDetails to true to see more information about each entity