using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Back-compat alias for the generated ListPayment tool. Schema mirrors
    /// <see cref="AspireAPI.Handlers.ListPaymentsHandler"/>: GET /Payments with
    /// OData query support. Legacy (query, expand, pageSize, pageNumber,
    /// useCache, status, contactId) schema retired together with its broken
    /// hand-written handler.
    /// </summary>
    public sealed class ListPaymentsToolDefinition : IToolDefinition
    {
        public string Name => "ListPayments";
        public string Description => "GET /Payments with OData query support (back-compat alias for ListPayment).";

        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(BackCompatToolSchemas.OdataCollectionSchema, cancellationToken);
    }
}
