using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Models;

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for handling date range operations and calculations
    /// </summary>
    public class DateRangeService
    {
        private readonly ILogger<DateRangeService> _logger;

        public DateRangeService(ILogger<DateRangeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses a date range query into start and end dates
        /// </summary>
        /// <param name="query">The date range query</param>
        /// <returns>A tuple containing start and end dates</returns>
        public (DateTime Start, DateTime End) ParseDateRange(DateRangeQuery query)
        {
            if (query == null)
            {
                // Default to current month if no query is provided
                var today = DateTime.Today;
                return (new DateTime(today.Year, today.Month, 1), today);
            }

            if (query.Type?.ToLower() == "custom" && query.StartDate.HasValue && query.EndDate.HasValue)
            {
                return (query.StartDate.Value, query.EndDate.Value);
            }
            
            return CalculateDateRange(query.Type ?? "thisMonth");
        }

        /// <summary>
        /// Calculates a date range based on a predefined type
        /// </summary>
        /// <param name="dateRangeType">The type of date range (e.g., thisWeek, lastMonth)</param>
        /// <returns>A tuple containing start and end dates</returns>
        public (DateTime Start, DateTime End) CalculateDateRange(string dateRangeType)
        {
            var today = DateTime.Today;
            
            switch (dateRangeType.ToLowerInvariant())
            {
                case "thisweek":
                    var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                    var thisWeekEnd = thisWeekStart.AddDays(6);
                    return (thisWeekStart, thisWeekEnd);
                    
                case "lastweek":
                    var lastWeekStart = today.AddDays(-(int)today.DayOfWeek - 7);
                    var lastWeekEnd = lastWeekStart.AddDays(6);
                    return (lastWeekStart, lastWeekEnd);
                    
                case "thismonth":
                    var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                    var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);
                    return (thisMonthStart, thisMonthEnd);
                    
                case "lastmonth":
                    var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                    return (lastMonthStart, lastMonthEnd);
                    
                case "lastquarter":
                    var currentQuarter = (today.Month - 1) / 3 + 1;
                    var lastQuarter = currentQuarter - 1;
                    var lastQuarterYear = today.Year;
                    
                    if (lastQuarter < 1)
                    {
                        lastQuarter = 4;
                        lastQuarterYear--;
                    }
                    
                    var lastQuarterStart = new DateTime(
                        lastQuarterYear, (lastQuarter - 1) * 3 + 1, 1);
                    var lastQuarterEnd = new DateTime(
                        lastQuarterYear, lastQuarter * 3, 1).AddMonths(1).AddDays(-1);
                    
                    return (lastQuarterStart, lastQuarterEnd);
                    
                case "lastyear":
                    var lastYearStart = new DateTime(today.Year - 1, 1, 1);
                    var lastYearEnd = new DateTime(today.Year - 1, 12, 31);
                    return (lastYearStart, lastYearEnd);
                    
                case "yeartodate":
                    var yearToDateStart = new DateTime(today.Year, 1, 1);
                    return (yearToDateStart, today);
                    
                default:
                    // Default to this month
                    var defaultMonthStart = new DateTime(today.Year, today.Month, 1);
                    var defaultMonthEnd = defaultMonthStart.AddMonths(1).AddDays(-1);
                    return (defaultMonthStart, defaultMonthEnd);
            }
        }

        /// <summary>
        /// Gets a date range from parameters dictionary
        /// </summary>
        /// <param name="parameters">Dictionary containing date parameters</param>
        /// <returns>A tuple containing start and end dates</returns>
        public (DateTime Start, DateTime End) GetDateRangeFromParameters(Dictionary<string, string> parameters)
        {
            _logger.LogInformation("Getting date range from parameters");
            
            if (parameters == null)
            {
                // Default to current month if no parameters are provided
                return CalculateDateRange("thisMonth");
            }

            // Check for date range type
            if (parameters.TryGetValue("dateRangeType", out var dateRangeType) && !string.IsNullOrEmpty(dateRangeType))
            {
                return CalculateDateRange(dateRangeType);
            }

            // Check for explicit start and end dates
            if (parameters.TryGetValue("startDate", out var startDateStr) &&
                parameters.TryGetValue("endDate", out var endDateStr))
            {
                if (DateTime.TryParse(startDateStr, out var startDate) &&
                    DateTime.TryParse(endDateStr, out var endDate))
                {
                    return (startDate, endDate);
                }
            }

            // Check for relative date range (days, weeks, months)
            if (parameters.TryGetValue("lastDays", out var lastDaysStr) &&
                int.TryParse(lastDaysStr, out var lastDays))
            {
                var today = DateTime.Today;
                return (today.AddDays(-lastDays), today);
            }
            
            if (parameters.TryGetValue("lastWeeks", out var lastWeeksStr) &&
                int.TryParse(lastWeeksStr, out var lastWeeks))
            {
                var today = DateTime.Today;
                return (today.AddDays(-(int)today.DayOfWeek).AddDays(-7 * lastWeeks), today);
            }
            
            if (parameters.TryGetValue("lastMonths", out var lastMonthsStr) &&
                int.TryParse(lastMonthsStr, out var lastMonths))
            {
                var today = DateTime.Today;
                return (new DateTime(today.Year, today.Month, 1).AddMonths(-lastMonths), today);
            }

            // Default to this month if no valid parameters are found
            return CalculateDateRange("thisMonth");
        }
    }
}