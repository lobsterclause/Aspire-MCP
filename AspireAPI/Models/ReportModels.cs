using System;
using System.Collections.Generic;

namespace AspireAPI.Models
{
    /// <summary>
    /// Defines a report to be generated
    /// </summary>
    public class ReportDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateId { get; set; }
        public string OutputFormat { get; set; } = "json";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public List<DataSource> DataSources { get; set; } = new List<DataSource>();
        public List<string> Columns { get; set; } = new List<string>();
        public FilterGroup Filters { get; set; }
        public List<CalculationDefinition> Calculations { get; set; } = new List<CalculationDefinition>();
        public List<string> GroupBy { get; set; } = new List<string>();
        public List<AggregationDefinition> Aggregations { get; set; } = new List<AggregationDefinition>();
        public List<SortDefinition> SortBy { get; set; } = new List<SortDefinition>();
        public VisualizationSpec Visualization { get; set; }
    }

    /// <summary>
    /// Defines a data source for a report
    /// </summary>
    public class DataSource
    {
        public string EntityType { get; set; }
        public bool IsPrimary { get; set; }
        public string JoinType { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public JoinCondition JoinCondition { get; set; }
        public string RightPrefix { get; set; }
    }

    /// <summary>
    /// Defines a condition for joining data sources
    /// </summary>
    public class JoinCondition
    {
        public string LeftField { get; set; }
        public string RightField { get; set; }
        public string ComparisonOperator { get; set; } = "eq";
    }

    /// <summary>
    /// Defines a filter group for reports
    /// </summary>
    public class FilterGroup
    {
        public string LogicalOperator { get; set; } = "and";
        public List<FilterCondition> Conditions { get; set; } = new List<FilterCondition>();
        public List<FilterGroup> Groups { get; set; } = new List<FilterGroup>();
    }

    /// <summary>
    /// Defines a filter condition for reports
    /// </summary>
    public class FilterCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; } = "eq";
        public object Value { get; set; }
        public List<object> Values { get; set; }
    }

    /// <summary>
    /// Defines a calculation for a report
    /// </summary>
    public class CalculationDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Formula { get; set; }
        public string DerivedFrom { get; set; }
        public string CustomFunction { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines an aggregation for a report
    /// </summary>
    public class AggregationDefinition
    {
        public string Name { get; set; }
        public string Function { get; set; }
        public string Field { get; set; }
        public decimal? Percentile { get; set; }
    }

    /// <summary>
    /// Defines a sort order for a report
    /// </summary>
    public class SortDefinition
    {
        public string Field { get; set; }
        public string Direction { get; set; } = "asc";
    }

    /// <summary>
    /// Defines visualization specifications for a report
    /// </summary>
    public class VisualizationSpec
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
        public List<VisualizationSeries> Series { get; set; } = new List<VisualizationSeries>();
        public VisualizationAxis XAxis { get; set; }
        public VisualizationAxis YAxis { get; set; }
        public List<string> Colors { get; set; } = new List<string>();
        public Dictionary<string, object> Legend { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines a series for visualization
    /// </summary>
    public class VisualizationSeries
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string Type { get; set; }
        public string YAxisIndex { get; set; } = "0";
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines an axis for visualization
    /// </summary>
    public class VisualizationAxis
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public bool ShowGrid { get; set; } = true;
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines a response from a report operation
    /// </summary>
    public class ReportResponse
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string DownloadUrl { get; set; }
    }
}