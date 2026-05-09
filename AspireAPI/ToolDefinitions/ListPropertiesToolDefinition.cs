using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Back-compat alias for the generated ListProperty tool. Schema mirrors
    /// <see cref="AspireAPI.Handlers.ListPropertiesHandler"/>: GET /Properties
    /// with OData query support. Legacy (query, ODataQuery, includeRelated,
    /// pageNumber, pageSize) schema retired together with its broken handler.
    /// </summary>
    public sealed class ListPropertiesToolDefinition : IToolDefinition
    {
        public string Name => "ListProperties";
        public string Description => "GET /Properties with OData query support (back-compat alias for ListProperty).";

        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(BackCompatToolSchemas.OdataCollectionSchema, cancellationToken);
    }
}
