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
    public class ListContactsToolDefinition : IToolDefinition
    {
        public string Name => "ListContacts";
        public string Description => "List contacts filtered by type (customer, vendor, employee, or all) with optional search";

        /// <summary>
        /// Input model for ListContacts tool
        /// </summary>
        public class ListContactsInput
        {
            [JsonPropertyName("type")]
            [Description("Type of contacts to list")]
            [Required]
            public string Type { get; set; }
            
            [JsonPropertyName("search")]
            [Description("Search term to filter contacts (optional)")]
            public string Search { get; set; }
            
            [JsonPropertyName("pageNumber")]
            [Description("Page number for pagination (default: 1)")]
            public int? PageNumber { get; set; }
            
            [JsonPropertyName("pageSize")]
            [Description("Page size for pagination (default: 50)")]
            public int? PageSize { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Use proper NJsonSchema approach to generate schema from type
            return await Task.FromResult(JsonSchema.FromType<ListContactsInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true }));
        }
    }
}