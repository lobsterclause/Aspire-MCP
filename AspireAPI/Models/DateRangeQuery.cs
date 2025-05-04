using System;

namespace AspireAPI.Models
{
    /// <summary>
    /// Date range query for filtering data by date
    /// </summary>
    public class DateRangeQuery
    {
        /// <summary>
        /// Type of date range (e.g., thisWeek, lastMonth, custom)
        /// </summary>
        public string Type { get; set; } = "thisMonth";
        
        /// <summary>
        /// Start date for custom date range
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// End date for custom date range
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Optionally include current day in the range
        /// </summary>
        public bool IncludeToday { get; set; } = true;
        
        /// <summary>
        /// Fields to apply the date range to
        /// </summary>
        public string DateField { get; set; } = "date";
        
        /// <summary>
        /// Creates a date range query for "this month"
        /// </summary>
        public static DateRangeQuery ThisMonth() => new DateRangeQuery { Type = "thisMonth" };
        
        /// <summary>
        /// Creates a date range query for "last month"
        /// </summary>
        public static DateRangeQuery LastMonth() => new DateRangeQuery { Type = "lastMonth" };
        
        /// <summary>
        /// Creates a date range query for "this week"
        /// </summary>
        public static DateRangeQuery ThisWeek() => new DateRangeQuery { Type = "thisWeek" };
        
        /// <summary>
        /// Creates a date range query for "last week"
        /// </summary>
        public static DateRangeQuery LastWeek() => new DateRangeQuery { Type = "lastWeek" };
        
        /// <summary>
        /// Creates a date range query for "last quarter"
        /// </summary>
        public static DateRangeQuery LastQuarter() => new DateRangeQuery { Type = "lastQuarter" };
        
        /// <summary>
        /// Creates a date range query for "last year"
        /// </summary>
        public static DateRangeQuery LastYear() => new DateRangeQuery { Type = "lastYear" };
        
        /// <summary>
        /// Creates a date range query for "year to date"
        /// </summary>
        public static DateRangeQuery YearToDate() => new DateRangeQuery { Type = "yearToDate" };
        
        /// <summary>
        /// Creates a custom date range query
        /// </summary>
        public static DateRangeQuery Custom(DateTime startDate, DateTime endDate) => new DateRangeQuery
        {
            Type = "custom",
            StartDate = startDate,
            EndDate = endDate
        };
    }
}