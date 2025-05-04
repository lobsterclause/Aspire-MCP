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
    public class ListJobsToolDefinition : IToolDefinition
    {
        public string Name => "ListJobs";
        public string Description => "List jobs from Aspire Cloud API";

        /// <summary>
        /// Input model for ListJobs tool
        /// </summary>
        public class ListJobsInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string? Search { get; set; }
            
            [JsonPropertyName("status")]
            [Description("Filter by job status (optional)")]
            public string? Status { get; set; }
            
            [JsonPropertyName("contactId")]
            [Description("Filter by contact ID (optional)")]
            public string? ContactId { get; set; }
            
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
            return await JsonSchema.FromTypeAsync<ListJobsInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}