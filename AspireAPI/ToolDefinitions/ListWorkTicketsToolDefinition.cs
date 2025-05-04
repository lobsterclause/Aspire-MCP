using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListWorkTicketsToolDefinition : IToolDefinition
    {
        public string Name => "ListWorkTickets";
        public string Description => "List work tickets from Aspire Cloud API";

        /// <summary>
        /// Input model for ListWorkTickets tool
        /// </summary>
        public class ListWorkTicketsInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListWorkTicketsInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true }, cancellationToken);
        }
    }
}