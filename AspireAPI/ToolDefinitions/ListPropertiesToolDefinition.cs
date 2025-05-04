using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListPropertiesToolDefinition : IToolDefinition
    {
        public string Name => "ListProperties";
        public string Description => "Lists property records from Aspire Cloud API";

        /// <summary>
        /// Input model for ListProperties tool
        /// </summary>
        public class ListPropertiesInput
        {
            [JsonPropertyName("query")]
            [Description("Search query (optional)")]
            public string Query { get; set; }
            
            [JsonPropertyName("contactId")]
            [Description("Filter by contact/owner ID (optional)")]
            public string ContactId { get; set; }
            
            [JsonPropertyName("oDataQuery")]
            [Description("Optional OData filter/sort query")]
            public string ODataQuery { get; set; }
            
            [JsonPropertyName("pageNumber")]
            [Description("Page number for pagination (default: 1)")]
            public int? PageNumber { get; set; }
            
            [JsonPropertyName("pageSize")]
            [Description("Page size for pagination (default: 50)")]
            public int? PageSize { get; set; }
            
            [JsonPropertyName("includeRelated")]
            [Description("Include related data like owner, location, agent (default: false)")]
            public bool? IncludeRelated { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Use proper NJsonSchema approach to generate schema from type
            return await JsonSchema.FromTypeAsync<ListPropertiesInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}