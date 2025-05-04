using System;
using System.Collections.Generic;

using AspireAPI.Models; // Added using directive for Models
namespace AspireAPI.Trend
{
    /// <summary>
    /// Result of trend analysis
    /// </summary>
    public class TrendResult
    {
        /// <summary>
        /// Type of entity analyzed
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Metric analyzed
        /// </summary>
        public string Metric { get; set; }

        /// <summary>
        /// Time dimension used for grouping
        /// </summary>
        public string TimeDimension { get; set; }

        /// <summary>
        /// Start date of the analysis period
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the analysis period
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Overall data points
        /// </summary>
        public List<TrendDataPoint> DataPoints { get; set; } = new List<TrendDataPoint>();

        /// <summary>
        /// Data points grouped by the specified field
        /// </summary>
        public Dictionary<string, List<TrendDataPoint>> GroupedData { get; set; } = new Dictionary<string, List<TrendDataPoint>>();

        /// <summary>
        /// Detected trends in the data
        /// </summary>
        public List<TrendPattern> DetectedTrends { get; set; } = new List<TrendPattern>();

        /// <summary>
        /// Summary statistics for the data
        /// </summary>
        public TrendSummary Summary { get; set; } = new TrendSummary();

        /// <summary>
        /// When the analysis was performed
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}