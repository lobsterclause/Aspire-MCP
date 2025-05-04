using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Input model for the Generate Report tool
    /// </summary>
    public class GenerateReportInput
    {
        /// <summary>
        /// Name of the report
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the report
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional ID of a saved report template to use
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Data sources to include in the report
        /// </summary>
        public List<ReportDataSource> DataSources { get; set; }

        /// <summary>
        /// Filters to apply (complex structure)
        /// </summary>
        public Dictionary<string, object> Filters { get; set; }

        /// <summary>
        /// Calculated fields
        /// </summary>
        public List<ReportCalculation> Calculations { get; set; }

        /// <summary>
        /// Fields to group by
        /// </summary>
        public List<string> GroupBy { get; set; }

        /// <summary>
        /// Aggregations for grouped data
        /// </summary>
        public List<ReportAggregation> Aggregations { get; set; }

        /// <summary>
        /// Fields to sort by
        /// </summary>
        public List<ReportSortField> SortBy { get; set; }

        /// <summary>
        /// Columns to include
        /// </summary>
        public List<string> Columns { get; set; }

        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Output format
        /// </summary>
        public string OutputFormat { get; set; } = "json";

        /// <summary>
        /// Visualization specification
        /// </summary>
        public ReportVisualization Visualization { get; set; }
    }

    /// <summary>
    /// Data source definition for a report
    /// </summary>
    public class ReportDataSource
    {
        /// <summary>
        /// Type of entity (e.g., jobs, invoices, contacts)
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Parameters for fetching the data
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// Calculation definition for a report
    /// </summary>
    public class ReportCalculation
    {
        /// <summary>
        /// Name of the calculated field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Formula for calculation
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Output data type
        /// </summary>
        public string DataType { get; set; }
    }

    /// <summary>
    /// Aggregation definition for a report
    /// </summary>
    public class ReportAggregation
    {
        /// <summary>
        /// Field to aggregate
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Function to apply (sum, avg, count, etc.)
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Name for the aggregated result
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Sort field definition for a report
    /// </summary>
    public class ReportSortField
    {
        /// <summary>
        /// Field to sort by
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Direction (asc or desc)
        /// </summary>
        public string Direction { get; set; } = "asc";
    }

    /// <summary>
    /// Visualization definition for a report
    /// </summary>
    public class ReportVisualization
    {
        /// <summary>
        /// Type of visualization
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Options for visualization
        /// </summary>
        public Dictionary<string, object> Options { get; set; }
    }

    /// <summary>
    /// Tool definition for generating advanced reports
    /// </summary>
    public class GenerateReportToolDefinition : IToolDefinition
    {
        private readonly ILogger<GenerateReportToolDefinition> _logger;

        public GenerateReportToolDefinition(ILogger<GenerateReportToolDefinition> logger)
        {
            _logger = logger;
        }

        public string Name => "generate_report";
        
        public string Description => "Generate an advanced custom report with filterable, sortable data from Aspire, supporting multi-entity reports, complex calculations, and various output formats.";
        
        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Use JsonSchema.FromTypeAsync to generate schema from the input class
            return await JsonSchema.FromTypeAsync<GenerateReportInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}