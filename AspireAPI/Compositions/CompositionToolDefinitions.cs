using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.Compositions
{
    /// <summary>
    /// Tool definitions for the four hand-written domain compositions. Each pairs
    /// with a *Handler in this folder. Schemas are inlined here (small enough that
    /// a per-tool file would be more clutter than clarity).
    /// </summary>
    public abstract class CompositionToolDefinitionBase : IToolDefinition
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        protected abstract string SchemaJson { get; }
        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(SchemaJson, cancellationToken);
    }

    public sealed class GetJobLifecycleToolDefinition : CompositionToolDefinitionBase
    {
        public override string Name => "GetJobLifecycle";
        public override string Description =>
            "[composition] Returns a single timeline-shaped payload for one job: the job " +
            "record + every related opportunity, work ticket, invoice, and payment. " +
            "Replaces 5+ raw tool calls with one round-trip.";
        protected override string SchemaJson => """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "jobId": { "type": "string", "description": "Aspire JobID (numeric or string form accepted)." }
  },
  "required": ["jobId"]
}
""";
    }

    public sealed class GetCustomer360ToolDefinition : CompositionToolDefinitionBase
    {
        public override string Name => "GetCustomer360";
        public override string Description =>
            "[composition] Returns one customer view: contact + properties + opportunities + " +
            "invoices + payments. Replaces 5 raw tool calls with one round-trip.";
        protected override string SchemaJson => """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "contactId": { "type": "string", "description": "Aspire ContactID (numeric or string form accepted)." }
  },
  "required": ["contactId"]
}
""";
    }

    public sealed class RenderScheduleBoardToolDefinition : CompositionToolDefinitionBase
    {
        public override string Name => "RenderScheduleBoard";
        public override string Description =>
            "[composition] Returns a calendar-grid-shaped payload of WorkTicketVisits for a " +
            "given date, grouped by RouteID. Optionally filterable by branch.";
        protected override string SchemaJson => """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "date":     { "type": "string", "format": "date", "description": "ISO yyyy-MM-dd date to render." },
    "branchId": { "type": "string", "description": "Optional Aspire BranchID to scope the board." }
  },
  "required": ["date"]
}
""";
    }

    public sealed class ListChangedSinceToolDefinition : CompositionToolDefinitionBase
    {
        public override string Name => "ListChangedSince";
        public override string Description =>
            "[composition] Aspire has no webhooks; this fans out $filter=LastModifiedDateTime ge X " +
            "across the entity types you care about and returns {entity → records}. Defaults to " +
            "Contacts, Invoices, Jobs, Opportunities, Payments, Properties, Receipts, WorkTickets, " +
            "WorkTicketTimes; pass `entities` to override.";
        protected override string SchemaJson => """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "since":    { "type": "string", "format": "date-time", "description": "ISO 8601 timestamp." },
    "entities": {
      "type": "array",
      "items": { "type": "string" },
      "description": "Optional override of which entity collections to query."
    }
  },
  "required": ["since"]
}
""";
    }
}
