using System;
using AspireAPI.Models; // Added using directive for Models

namespace AspireAPI.Trend
{
    /// <summary>
    /// Summary statistics for trend data
    /// </summary>
    public class TrendSummary
    {
        /// <summary>
        /// Minimum value in the data
        /// </summary>
        public decimal MinValue { get; set; }

        /// <summary>
        /// Maximum value in the data
        /// </summary>
        public decimal MaxValue { get; set; }

        /// <summary>
        /// Average value in the data
        /// </summary>
        public decimal AverageValue { get; set; }

        /// <summary>
        /// Total value across all data points
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Overall growth rate (first to last point)
        /// </summary>
        public decimal OverallGrowthRate { get; set; }

        /// <summary>
        /// Compound monthly growth rate
        /// </summary>
        public decimal CMGR { get; set; }

        /// <summary>
        /// Standard deviation of the data
        /// </summary>
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Number of data points
        /// </summary>
        public int DataPointCount { get; set; }
    }
}