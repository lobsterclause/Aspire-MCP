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
    public class GetTimeEntryReportToolDefinition : IToolDefinition
    {
        public string Name => "GetTimeEntryReport";
        public string Description => "Generate a time entry report for a client within a specified date range";

        /// <summary>
        /// Input model for GetTimeEntryReport tool
        /// </summary>
        public class GetTimeEntryReportInput
        {
            [JsonPropertyName("clientName")]
            [Description("Name of the client to generate the report for")]
            [Required]
            public string ClientName { get; set; }
            
            [JsonPropertyName("dateRange")]
            [Description("Date range for the report")]
            [Required]
            public string DateRange { get; set; }
            
            [JsonPropertyName("divisionName")]
            [Description("Name of the division to filter by (optional)")]
            public string DivisionName { get; set; }
            
            [JsonPropertyName("startDate")]
            [Description("Start date in yyyy-MM-dd format (required only when dateRange is 'custom')")]
            public string StartDate { get; set; }
            
            [JsonPropertyName("endDate")]
            [Description("End date in yyyy-MM-dd format (required only when dateRange is 'custom')")]
            public string EndDate { get; set; }
        }

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Use proper NJsonSchema approach to generate schema from type
            return await JsonSchema.FromTypeAsync<GetTimeEntryReportInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}