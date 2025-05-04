using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class GetScheduleBoardToolDefinition : IToolDefinition
    {
        public string Name => "GetScheduleBoard";
        public string Description => "Get schedule board data with optional filters";

        /// <summary>
        /// Input model for GetScheduleBoard tool
        /// </summary>
        public class GetScheduleBoardInput
        {
            [JsonPropertyName("branchId")]
            [Description("Branch ID (optional)")]
            public string BranchId { get; set; }
            
            [JsonPropertyName("divisionId")]
            [Description("Division ID (optional)")]
            public string DivisionId { get; set; }
            
            [JsonPropertyName("startDate")]
            [Description("Start date (YYYY-MM-DD)")]
            [Required]
            public string StartDate { get; set; }
            
            [JsonPropertyName("endDate")]
            [Description("End date (YYYY-MM-DD)")]
            [Required]
            public string EndDate { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<GetScheduleBoardInput>(
                new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}