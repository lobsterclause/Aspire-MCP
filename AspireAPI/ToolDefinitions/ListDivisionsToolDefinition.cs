using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListDivisionsToolDefinition : IToolDefinition
    {
        public string Name => "ListDivisions";
        public string Description => "List divisions from Aspire Cloud API";

        /// <summary>
        /// Input model for ListDivisions tool
        /// </summary>
        public class ListDivisionsInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListDivisionsInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}