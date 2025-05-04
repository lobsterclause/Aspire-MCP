using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListInventoryItemsToolDefinition : IToolDefinition
    {
        public string Name => "ListInventoryItems";
        public string Description => "List inventory items from Aspire Cloud API";

        /// <summary>
        /// Input model for ListInventoryItems tool
        /// </summary>
        public class ListInventoryItemsInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListInventoryItemsInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}