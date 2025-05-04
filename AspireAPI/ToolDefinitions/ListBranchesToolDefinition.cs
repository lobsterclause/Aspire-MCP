using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class ListBranchesToolDefinition : IToolDefinition
    {
        public string Name => "ListBranches";
        public string Description => "List branches from Aspire Cloud API";

        /// <summary>
        /// Input model for ListBranches tool
        /// </summary>
        public class ListBranchesInput
        {
            [JsonPropertyName("search")]
            [Description("Search term (optional)")]
            public string Search { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<ListBranchesInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}