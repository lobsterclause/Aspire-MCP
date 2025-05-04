using System;

namespace AspireAPI.Trend
{
    /// <summary>
    /// Data point in a trend
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// Date of the data point
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Value of the data point
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Moving average value (if calculated)
        /// </summary>
        public decimal? MovingAverage { get; set; }

        /// <summary>
        /// Percent change from previous data point
        /// </summary>
        public decimal? PercentChange { get; set; }

        /// <summary>
        /// Whether this is a key point (peak, valley, etc.)
        /// </summary>
        public bool IsKeyPoint { get; set; }

        /// <summary>
        /// Label for key points
        /// </summary>
        public string KeyPointLabel { get; set; }
        
        /// <summary>
        /// Number of items represented in this data point
        /// </summary>
        public int ItemCount { get; set; }
    }
}