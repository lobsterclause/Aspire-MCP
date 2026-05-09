using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Back-compat alias for the generated ListContact tool. Schema mirrors
    /// <see cref="AspireAPI.Handlers.ListContactsHandler"/>: GET /Contacts with
    /// OData query support. The legacy (type, search, pageNumber, pageSize)
    /// schema was retired together with the broken DataFetchService-based
    /// handler; clients should migrate to OData expressions
    /// (e.g. $filter=Type eq 'customer', $top, $skip).
    /// </summary>
    public sealed class ListContactsToolDefinition : IToolDefinition
    {
        public string Name => "ListContacts";
        public string Description => "GET /Contacts with OData query support (back-compat alias for ListContact).";

        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(BackCompatToolSchemas.OdataCollectionSchema, cancellationToken);
    }
}
