namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Schemas shared across the four hand-written back-compat tools
    /// (ListContacts, ListJobs, ListPayments, ListProperties). Each is a thin
    /// wrapper around the corresponding generated tool — they advertise the
    /// same OData query surface so MCP clients see a consistent input shape.
    /// </summary>
    internal static class BackCompatToolSchemas
    {
        internal const string OdataCollectionSchema = """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": true,
  "properties": {
    "$filter":  { "type": "string",  "description": "OData $filter expression, e.g. \"Type eq 'customer'\"." },
    "$top":     { "type": "integer", "description": "Maximum number of records to return." },
    "$skip":    { "type": "integer", "description": "Number of records to skip (offset for pagination)." },
    "$orderby": { "type": "string",  "description": "OData $orderby expression." },
    "$select":  { "type": "string",  "description": "Comma-separated list of fields to project." },
    "$expand":  { "type": "string",  "description": "Comma-separated list of related entities to expand." }
  }
}
""";
    }
}
