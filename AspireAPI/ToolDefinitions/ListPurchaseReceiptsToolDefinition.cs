using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListPurchaseReceiptsToolDefinition : IToolDefinition
    {
        public string Name => "ListPurchaseReceipts";
        public string Description => "List purchase receipts from Aspire Cloud API";

        /// <summary>
        /// Input model for ListPurchaseReceipts tool
        /// </summary>
        public class ListPurchaseReceiptsInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListPurchaseReceiptsInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true }, cancellationToken);
        }
    }
}