using System;
using System.Collections.Generic;

namespace AspireAPI.Comparison
{
    /// <summary>
    /// Models for comparison functionality within the AspireAPI
    /// </summary>
    public class ComparisonResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, ComparisonMetric> Metrics { get; set; } = new Dictionary<string, ComparisonMetric>();
        public ComparisonPeriod BasePeriod { get; set; }
        public ComparisonPeriod ComparisonPeriod { get; set; }
        public string EntityType { get; set; }
        public ComparisonType Type { get; set; }
        
        /// <summary>
        /// Dimension being compared
        /// </summary>
        public string Dimension { get; set; }
        
        /// <summary>
        /// First value in the comparison
        /// </summary>
        public string FirstValue { get; set; }
        
        /// <summary>
        /// Second value in the comparison
        /// </summary>
        public string SecondValue { get; set; }
        
        /// <summary>
        /// Summary of the comparison results
        /// </summary>
        public ComparisonSummary Summary { get; set; }
        
        /// <summary>
        /// Detailed comparison results
        /// </summary>
        public List<ComparisonDetail> Details { get; set; } = new List<ComparisonDetail>();
        
        /// <summary>
        /// When the comparison was performed
        /// </summary>
        public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a metric value and its change in a comparison
    /// </summary>
    public class ComparisonMetric
    {
        public string Name { get; set; }
        public decimal BaseValue { get; set; }
        public decimal ComparisonValue { get; set; }
        public decimal Change { get; set; }
        public decimal PercentageChange { get; set; }
        public TrendDirection Direction { get; set; }
        public ComparisonSeverity Severity { get; set; }
    }

    /// <summary>
    /// Represents a period for comparison
    /// </summary>
    public class ComparisonPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Enum for trend direction
    /// </summary>
    public enum TrendDirection
    {
        Up,
        Down,
        Unchanged
    }

    /// <summary>
    /// Enum for comparison type
    /// </summary>
    public enum ComparisonType
    {
        TimePeriod,
        Division,
        Branch,
        Employee,
        Custom
    }

    /// <summary>
    /// Enum for comparison severity
    /// </summary>
    public enum ComparisonSeverity
    {
        Positive,
        Neutral,
        Warning,
        Critical
    }

    /// <summary>
    /// Configuration for a comparison
    /// </summary>
    public class ComparisonConfiguration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public ComparisonType Type { get; set; }
        public string EntityType { get; set; }
        public List<string> MetricsToCompare { get; set; } = new List<string>();
        public Dictionary<string, SeverityThreshold> SeverityThresholds { get; set; } = new Dictionary<string, SeverityThreshold>();
    }

    /// <summary>
    /// Threshold configuration for determining severity
    /// </summary>
    public class SeverityThreshold
    {
        public decimal WarningThreshold { get; set; }
        public decimal CriticalThreshold { get; set; }
        public bool IsPositiveWhenIncreasing { get; set; }
    }

    /// <summary>
    /// Filter condition for filtering comparison data
    /// </summary>
    public class FilterCondition
    {
        /// <summary>
        /// Field to filter on
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Operator for the filter (eq, ne, gt, lt, etc.)
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Value to compare against
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Parameters for performing comparisons
    /// </summary>
    public class ComparisonParameters
    {
        /// <summary>
        /// Type of comparison to perform
        /// </summary>
        public ComparisonType Type { get; set; } = ComparisonType.TimePeriod;

        /// <summary>
        /// Entity type to compare
        /// </summary>
        public string EntityType { get; set; } = "Jobs";

        /// <summary>
        /// Start date for base period
        /// </summary>
        public DateTime BasePeriodStartDate { get; set; }

        /// <summary>
        /// End date for base period
        /// </summary>
        public DateTime BasePeriodEndDate { get; set; }

        /// <summary>
        /// Start date for comparison period
        /// </summary>
        public DateTime ComparisonPeriodStartDate { get; set; }

        /// <summary>
        /// End date for comparison period
        /// </summary>
        public DateTime ComparisonPeriodEndDate { get; set; }

        /// <summary>
        /// IDs to filter by (e.g. division IDs, branch IDs)
        /// </summary>
        public List<string> FilterIds { get; set; } = new List<string>();
        
        /// <summary>
        /// Metrics to include in comparison
        /// </summary>
        public List<string> Metrics { get; set; } = new List<string>();
        
        /// <summary>
        /// Dimension to compare by
        /// </summary>
        public string Dimension { get; set; }
        
        /// <summary>
        /// First value for the comparison
        /// </summary>
        public string FirstValue { get; set; }
        
        /// <summary>
        /// First period start date as string
        /// </summary>
        public string FirstStartDate { get; set; }
        
        /// <summary>
        /// First period end date as string
        /// </summary>
        public string FirstEndDate { get; set; }
        
        /// <summary>
        /// Second value for the comparison
        /// </summary>
        public string SecondValue { get; set; }
        
        /// <summary>
        /// Second period start date as string
        /// </summary>
        public string SecondStartDate { get; set; }
        
        /// <summary>
        /// Second period end date as string
        /// </summary>
        public string SecondEndDate { get; set; }
        
        /// <summary>
        /// Field to group results by
        /// </summary>
        public string GroupBy { get; set; }
        
        /// <summary>
        /// Filter conditions to apply to the data
        /// </summary>
        public List<AspireAPI.Models.FilterCondition> Filters { get; set; } = new List<AspireAPI.Models.FilterCondition>();
    }

    /// <summary>
    /// Details for a specific metric comparison
    /// </summary>
    public class ComparisonDetail
    {
        /// <summary>
        /// Name of the metric
        /// </summary>
        public string MetricName { get; set; }

        /// <summary>
        /// Value from base period
        /// </summary>
        public decimal BaseValue { get; set; }

        /// <summary>
        /// Value from comparison period
        /// </summary>
        public decimal ComparisonValue { get; set; }

        /// <summary>
        /// Absolute change in values
        /// </summary>
        public decimal Change { get; set; }

        /// <summary>
        /// Percentage change
        /// </summary>
        public decimal PercentageChange { get; set; }

        /// <summary>
        /// Direction of the trend (up, down, unchanged)
        /// </summary>
        public TrendDirection Direction { get; set; }

        /// <summary>
        /// Severity level of the change
        /// </summary>
        public ComparisonSeverity Severity { get; set; }

        /// <summary>
        /// Metric being compared
        /// </summary>
        public string Metric { get; set; }

        /// <summary>
        /// First value in the comparison
        /// </summary>
        public decimal FirstValue { get; set; }

        /// <summary>
        /// Second value in the comparison
        /// </summary>
        public decimal SecondValue { get; set; }

        /// <summary>
        /// The difference between the two values
        /// </summary>
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// High-level summary of comparison results
    /// </summary>
    public class ComparisonSummary
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of comparison
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of comparison performed
        /// </summary>
        public ComparisonType Type { get; set; }

        /// <summary>
        /// Type of entity compared
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Base period information
        /// </summary>
        public ComparisonPeriod BasePeriod { get; set; }

        /// <summary>
        /// Comparison period information
        /// </summary>
        public ComparisonPeriod ComparisonPeriod { get; set; }

        /// <summary>
        /// Details for each metric compared
        /// </summary>
        public List<ComparisonDetail> Details { get; set; } = new List<ComparisonDetail>();

        /// <summary>
        /// Date the comparison was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Key differences identified in the comparison
        /// </summary>
        public List<string> KeyDifferences { get; set; } = new List<string>();
        
        /// <summary>
        /// Overall trend as string 
        /// </summary>
        public string OverallTrend { get; set; }
    }
}