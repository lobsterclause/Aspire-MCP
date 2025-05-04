using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI
{
    // IToolDefinition interface is now defined in McpTypes.cs

    /// <summary>
    /// Base class for tool definitions
    /// </summary>
    public abstract class BaseToolDefinition : IToolDefinition
    {
        /// <summary>
        /// Gets the name of the tool
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// Gets the description of the tool
        /// </summary>
        public abstract string Description { get; }
        
        /// <summary>
        /// Gets the schema for the tool's input parameters
        /// </summary>
        /// <returns>JSON schema object</returns>
        public virtual async Task<NJsonSchema.JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            string jsonSchema = GetJsonSchema();
            return await NJsonSchema.JsonSchema.FromJsonAsync(jsonSchema, cancellationToken);
        }
        
        /// <summary>
        /// Gets the JSON schema for the tool
        /// </summary>
        /// <returns>JSON schema string</returns>
        protected abstract string GetJsonSchema();
    }
}