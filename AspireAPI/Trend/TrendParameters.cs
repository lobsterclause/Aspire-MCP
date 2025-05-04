using System;
using System.Collections.Generic;
using ModelContextProtocol.Protocol.Types; // Assuming FilterCondition is defined here
using AspireAPI.Models; // Added using directive for Models

namespace AspireAPI.Trend
{
    /// <summary>
    /// Parameters for trend analysis
    /// </summary>
    public class TrendParameters
    {
        /// <summary>
        /// Type of entity to analyze
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Start date for analysis
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// End date for analysis
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Metric to analyze
        /// </summary>
        public string Metric { get; set; } = "totalHours";

        /// <summary>
        /// Time dimension for grouping (daily, weekly, monthly, etc.)
        /// </summary>
        public string TimeDimension { get; set; } = "monthly";

        /// <summary>
        /// Optional field to group results by
        /// </summary>
        public string GroupBy { get; set; }

        /// <summary>
        /// Additional filters to apply
        /// </summary>
        public List<FilterCondition> Filters { get; set; } = new List<FilterCondition>();

        /// <summary>
        /// Number of periods for moving average calculation
        /// </summary>
        public int MovingAveragePeriod { get; set; } = 3;
    }
}