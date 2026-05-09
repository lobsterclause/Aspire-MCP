using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Back-compat alias for the generated ListJob tool. Schema mirrors
    /// <see cref="AspireAPI.Handlers.ListJobsHandler"/>: GET /Jobs with OData
    /// query support. Legacy (status, contactId, search, pageNumber, pageSize)
    /// schema retired together with its broken handler.
    /// </summary>
    public sealed class ListJobsToolDefinition : IToolDefinition
    {
        public string Name => "ListJobs";
        public string Description => "GET /Jobs with OData query support (back-compat alias for ListJob).";

        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(BackCompatToolSchemas.OdataCollectionSchema, cancellationToken);
    }
}
