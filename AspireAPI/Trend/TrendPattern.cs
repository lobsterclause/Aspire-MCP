using System;

namespace AspireAPI.Trend
{
    /// <summary>
    /// Detected pattern in trend data
    /// </summary>
    public class TrendPattern
    {
        /// <summary>
        /// Type of pattern (Growth, Decline, Spike, Plateau)
        /// </summary>
        public string PatternType { get; set; }

        /// <summary>
        /// Start date of the pattern
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the pattern
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Magnitude of the pattern (percent change, etc.)
        /// </summary>
        public decimal Magnitude { get; set; }

        /// <summary>
        /// Human-readable description of the pattern
        /// </summary>
        public string Description { get; set; }
    }
}