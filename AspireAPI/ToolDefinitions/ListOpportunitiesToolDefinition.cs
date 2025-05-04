using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListOpportunitiesToolDefinition : IToolDefinition
    {
        public string Name => "ListOpportunities";
        public string Description => "List opportunities from Aspire Cloud API";

        /// <summary>
        /// Input model for ListOpportunities tool
        /// </summary>
        public class ListOpportunitiesInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListOpportunitiesInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true }, cancellationToken);
        }
    }
}