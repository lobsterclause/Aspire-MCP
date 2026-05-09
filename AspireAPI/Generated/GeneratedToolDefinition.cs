using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.Generated
{
    /// <summary>
    /// Base for code-generated tool definitions. Subclasses supply Name, Description, and a
    /// pre-rendered JSON schema string. We intentionally bypass NJsonSchema reflection-from-types
    /// because every generated tool has a different schema and we already have the OpenAPI source.
    /// The parsed JsonSchema is memoized per-instance — clients (especially Claude Desktop)
    /// call ListTools repeatedly and re-parsing 162 schemas on each call wastes ~1 MB of
    /// short-lived allocations.
    /// </summary>
    public abstract class GeneratedToolDefinition : IToolDefinition
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        protected abstract string JsonSchemaString { get; }

        private JsonSchema? _cached;
        private readonly object _cacheLock = new();

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            var existing = _cached;
            if (existing is not null) return existing;
            // Parse outside the lock to avoid holding it across an await on cold paths.
            var parsed = await JsonSchema.FromJsonAsync(JsonSchemaString, cancellationToken)
                .ConfigureAwait(false);
            lock (_cacheLock)
            {
                _cached ??= parsed;
                return _cached;
            }
        }
    }
}
