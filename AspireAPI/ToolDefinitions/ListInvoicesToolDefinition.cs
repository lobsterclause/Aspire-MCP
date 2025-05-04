using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListInvoicesToolDefinition : IToolDefinition
    {
        public string Name => "ListInvoices";
        public string Description => "List invoices from Aspire Cloud API";

        /// <summary>
        /// Input model for ListInvoices tool
        /// </summary>
        public class ListInvoicesInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListInvoicesInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true }, cancellationToken);
        }
    }
}