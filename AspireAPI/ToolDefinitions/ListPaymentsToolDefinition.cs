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
    public class ListPaymentsToolDefinition : IToolDefinition
    {
        public string Name => "ListPayments";
        public string Description => "List payments from Aspire Cloud API with advanced OData query support";

        /// <summary>
        /// Input model for ListPayments tool
        /// </summary>
        public class ListPaymentsInput
        {
            [JsonPropertyName("query")]
            [Description("OData query string (e.g., $filter=amount gt 100 and date ge 2023-01-01)")]
            public string Query { get; set; }
            
            [JsonPropertyName("expand")]
            [Description("Related entities to expand (comma-separated, e.g., 'invoice,contact')")]
            public string Expand { get; set; }
            
            [JsonPropertyName("pageSize")]
            [Description("Number of results per page (default: 100)")]
            public int? PageSize { get; set; }
            
            [JsonPropertyName("pageNumber")]
            [Description("Page number to retrieve (default: 1)")]
            public int? PageNumber { get; set; }
            
            [JsonPropertyName("useCache")]
            [Description("Whether to use cached data if available (default: true)")]
            public bool? UseCache { get; set; }
            
            [JsonPropertyName("status")]
            [Description("Filter by payment status (optional)")]
            public string Status { get; set; }
            
            [JsonPropertyName("contactId")]
            [Description("Filter by contact ID (optional)")]
            public string ContactId { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Use proper NJsonSchema approach to generate schema from type
            return await Task.FromResult(JsonSchema.FromType<ListPaymentsInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true }));
        }
    }
}