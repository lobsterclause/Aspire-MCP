using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI.Generated
{
    /// <summary>
    /// Base for code-generated tool definitions. Subclasses supply Name, Description, and a
    /// pre-rendered JSON schema string. We intentionally bypass NJsonSchema reflection-from-types
    /// because every generated tool has a different schema and we already have the OpenAPI source.
    /// </summary>
    public abstract class GeneratedToolDefinition : IToolDefinition
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        protected abstract string JsonSchemaString { get; }

        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
            => JsonSchema.FromJsonAsync(JsonSchemaString, cancellationToken);
    }
}
