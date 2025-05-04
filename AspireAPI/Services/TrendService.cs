using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Added missing using directive
using AspireAPI.Trend;
using AspireAPI.Models; // Added missing using directive

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for analyzing historical trends
    /// </summary>
    public class TrendService
    {
        private readonly AspireApiService _aspireApi;
        private readonly ILogger _logger;
        private readonly DataFetchService _dataFetcher;
        private readonly MetricCalculationService _metricCalculator; // Added MetricCalculationService

        public TrendService(AspireApiService aspireApi, ILogger logger, DataFetchService dataFetcher, MetricCalculationService metricCalculator) // Added DataFetchService and MetricCalculationService
        {
            _aspireApi = aspireApi;
            _logger = logger;
            _dataFetcher = dataFetcher; // Assigned injected service
            _metricCalculator = metricCalculator; // Assigned injected service
        }

        /// <summary>
        /// Analyze trends based on the specified parameters
        /// </summary>
        public async Task<Trend.TrendResult> AnalyzeTrendsAsync(
            Trend.TrendParameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                // Fetch data from Aspire API
                var data = await FetchEntityDataAsync(
                    parameters.EntityType,
                    parameters.StartDate,
                    parameters.EndDate,
                    parameters.Filters,
                    cancellationToken);

                // Create result object
                var result = new Trend.TrendResult
                {
                    EntityType = parameters.EntityType,
                    Metric = parameters.Metric,
                    TimeDimension = parameters.TimeDimension,
                    StartDate = DateTime.Parse(parameters.StartDate),
                    EndDate = DateTime.Parse(parameters.EndDate)
                };

                // Calculate overall trend
                CalculateOverallTrend(data, result, parameters);

                // Calculate grouped trends if groupBy is specified
                if (!string.IsNullOrEmpty(parameters.GroupBy))
                {
                    CalculateGroupedTrends(data, result, parameters);
                }

                // Apply moving average
                if (parameters.MovingAveragePeriod > 1)
                {
                    ApplyMovingAverage(result, parameters.MovingAveragePeriod);
                }

                // Detect trends in the data
                DetectTrends(result);

                // Calculate summary statistics
                CalculateSummaryStatistics(result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing trends for {parameters.EntityType}");
                throw;
            }
        }

        /// <summary>
        /// Apply moving average to the data points
        /// </summary>
        private void ApplyMovingAverage(TrendResult result, int period)
        {
            // Apply to overall data points
            if (result.DataPoints != null && result.DataPoints.Count > 0)
            {
                ApplyMovingAverageToSeries(result.DataPoints, period);
            }

            // Apply to each group's data points
            foreach (var groupName in result.GroupedData.Keys)
            {
                var dataPoints = result.GroupedData[groupName];
                if (dataPoints != null && dataPoints.Count > 0)
                {
                    ApplyMovingAverageToSeries(dataPoints, period);
                }
            }
        }

        /// <summary>
        /// Apply moving average to a single data series
        /// </summary>
        private void ApplyMovingAverageToSeries(List<TrendDataPoint> dataPoints, int period)
        {
            // Need at least 'period' data points
            if (dataPoints.Count < period)
            {
                return;
            }

            // Calculate moving average for each point
            for (int i = 0; i < dataPoints.Count; i++)
            {
                // Skip points that don't have enough prior data
                if (i < period - 1)
                {
                    continue;
                }

                // Calculate sum of values in the period
                decimal sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += dataPoints[i - j].Value;
                }

                // Calculate average
                dataPoints[i].MovingAverage = Math.Round(sum / period, 2);
            }
        }

        /// <summary>
        /// Detect trends in the data
        /// </summary>
        private void DetectTrends(TrendResult result)
        {
            // Only process overall data if available
            if (result.DataPoints == null || result.DataPoints.Count < 3)
            {
                return;
            }

            var dataPoints = result.DataPoints;
            var trends = new List<TrendPattern>();

            // Find local maximums and minimums
            FindKeyPoints(dataPoints);

            // Detect continuous growth periods
            DetectGrowthPeriods(dataPoints, trends);

            // Detect continuous decline periods
            DetectDeclinePeriods(dataPoints, trends);

            // Find significant spikes
            DetectSpikes(dataPoints, trends);

            // Detect plateaus (periods of stability)
            DetectPlateaus(dataPoints, trends);

            // Set detected trends
            result.DetectedTrends = trends;
        }

        /// <summary>
        /// Find key points in the data (peaks, valleys)
        /// </summary>
        private void FindKeyPoints(List<TrendDataPoint> dataPoints)
        {
            // Need at least 3 points to find peaks/valleys
            if (dataPoints.Count < 3)
            {
                return;
            }

            // Check each point (except first and last)
            for (int i = 1; i < dataPoints.Count - 1; i++)
            {
                var prev = dataPoints[i - 1].Value;
                var curr = dataPoints[i].Value;
                var next = dataPoints[i + 1].Value;

                // Check for a peak
                if (curr > prev && curr > next)
                {
                    dataPoints[i].IsKeyPoint = true;
                    dataPoints[i].KeyPointLabel = "Peak";
                }
                // Check for a valley
                else if (curr < prev && curr < next)
                {
                    dataPoints[i].IsKeyPoint = true;
                    dataPoints[i].KeyPointLabel = "Valley";
                }
            }

            // Also mark first and last points as key points
            if (dataPoints.Count > 0)
            {
                dataPoints[0].IsKeyPoint = true;
                dataPoints[0].KeyPointLabel = "Start";

                dataPoints[dataPoints.Count - 1].IsKeyPoint = true;
                dataPoints[dataPoints.Count - 1].KeyPointLabel = "End";
            }
        }

        /// <summary>
        /// Detect continuous growth periods in the data
        /// </summary>
        private void DetectGrowthPeriods(List<TrendDataPoint> dataPoints, List<TrendPattern> trends)
        {
            int startIndex = 0;
            bool inGrowthPeriod = false;

            for (int i = 1; i < dataPoints.Count; i++)
            {
                var curr = dataPoints[i].Value;
                var prev = dataPoints[i - 1].Value;

                // Check if we have growth
                bool isGrowing = curr > prev;

                // If we're starting a growth period
                if (isGrowing && !inGrowthPeriod)
                {
                    startIndex = i - 1;
                    inGrowthPeriod = true;
                }
                // If we're ending a growth period
                else if (!isGrowing && inGrowthPeriod)
                {
                    // Calculate the growth
                    var startValue = dataPoints[startIndex].Value;
                    var endValue = dataPoints[i - 1].Value;

                    // Only record significant growth (more than 10%)
                    if (startValue > 0 && (endValue - startValue) / startValue > 0.1m)
                    {
                        trends.Add(new TrendPattern
                        {
                            PatternType = "Growth",
                            StartDate = dataPoints[startIndex].Date,
                            EndDate = dataPoints[i - 1].Date,
                            Magnitude = Math.Round(((endValue - startValue) / startValue) * 100, 2),
                            Description = $"Growth of {Math.Round(((endValue - startValue) / startValue) * 100, 2)}% " +
                                        $"from {dataPoints[startIndex].Date:d} to {dataPoints[i - 1].Date:d}"
                        });
                    }

                    inGrowthPeriod = false;
                }
            }

            // If we're still in a growth period at the end
            if (inGrowthPeriod && startIndex < dataPoints.Count - 1)
            {
                // Calculate the growth
                var startValue = dataPoints[startIndex].Value;
                var endValue = dataPoints[dataPoints.Count - 1].Value;

                // Only record significant growth (more than 10%)
                if (startValue > 0 && (endValue - startValue) / startValue > 0.1m)
                {
                    trends.Add(new TrendPattern
                    {
                        PatternType = "Growth",
                        StartDate = dataPoints[startIndex].Date,
                        EndDate = dataPoints[dataPoints.Count - 1].Date,
                        Magnitude = Math.Round(((endValue - startValue) / startValue) * 100, 2),
                        Description = $"Growth of {Math.Round(((endValue - startValue) / startValue) * 100, 2)}% " +
                                    $"from {dataPoints[startIndex].Date:d} to {dataPoints[dataPoints.Count - 1].Date:d}"
                    });
                }
            }
        }

        /// <summary>
        /// Detect continuous decline periods in the data
        /// </summary>
        private void DetectDeclinePeriods(List<TrendDataPoint> dataPoints, List<TrendPattern> trends)
        {
            int startIndex = 0;
            bool inDeclinePeriod = false;

            for (int i = 1; i < dataPoints.Count; i++)
            {
                var curr = dataPoints[i].Value;
                var prev = dataPoints[i - 1].Value;

                // Check if we have decline
                bool isDeclining = curr < prev;

                // If we're starting a decline period
                if (isDeclining && !inDeclinePeriod)
                {
                    startIndex = i - 1;
                    inDeclinePeriod = true;
                }
                // If we're ending a decline period
                else if (!isDeclining && inDeclinePeriod)
                {
                    // Calculate the decline
                    var startValue = dataPoints[startIndex].Value;
                    var endValue = dataPoints[i - 1].Value;

                    // Only record significant decline (more than 10%)
                    if (startValue > 0 && (startValue - endValue) / startValue > 0.1m)
                    {
                        trends.Add(new TrendPattern
                        {
                            PatternType = "Decline",
                            StartDate = dataPoints[startIndex].Date,
                            EndDate = dataPoints[i - 1].Date,
                            Magnitude = Math.Round(((endValue - startValue) / startValue) * 100, 2),
                            Description = $"Decline of {Math.Round(((startValue - endValue) / startValue) * 100, 2)}% " +
                                        $"from {dataPoints[startIndex].Date:d} to {dataPoints[i - 1].Date:d}"
                        });
                    }

                    inDeclinePeriod = false;
                }
            }

            // If we're still in a decline period at the end
            if (inDeclinePeriod && startIndex < dataPoints.Count - 1)
            {
                // Calculate the decline
                var startValue = dataPoints[startIndex].Value;
                var endValue = dataPoints[dataPoints.Count - 1].Value;

                // Only record significant decline (more than 10%)
                if (startValue > 0 && (startValue - endValue) / startValue > 0.1m)
                {
                    trends.Add(new TrendPattern
                    {
                        PatternType = "Decline",
                        StartDate = dataPoints[startIndex].Date,
                        EndDate = dataPoints[dataPoints.Count - 1].Date,
                        Magnitude = Math.Round(((endValue - startValue) / startValue) * 100, 2),
                        Description = $"Decline of {Math.Round(((startValue - endValue) / startValue) * 100, 2)}% " +
                                    $"from {dataPoints[startIndex].Date:d} to {dataPoints[dataPoints.Count - 1].Date:d}"
                    });
                }
            }
        }

        /// <summary>
        /// Calculate summary statistics for the trend data
        /// </summary>
        private void CalculateSummaryStatistics(Trend.TrendResult result)
        {
            var dataPoints = result.DataPoints;

            if (dataPoints == null || dataPoints.Count == 0)
            {
                return;
            }

            // Calculate basic statistics
            result.Summary.MinValue = dataPoints.Min(p => p.Value);
            result.Summary.MaxValue = dataPoints.Max(p => p.Value);
            result.Summary.AverageValue = Math.Round(dataPoints.Average(p => p.Value), 2);
            result.Summary.TotalValue = dataPoints.Sum(p => p.Value);
            result.Summary.DataPointCount = dataPoints.Count;

            // Calculate overall growth rate
            var firstValue = dataPoints.First().Value;
            var lastValue = dataPoints.Last().Value;

            if (firstValue != 0)
            {
                result.Summary.OverallGrowthRate = Math.Round(((lastValue - firstValue) / firstValue) * 100, 2);
            }

            // Calculate standard deviation
            decimal mean = result.Summary.AverageValue;
            decimal sumOfSquaredDifferences = dataPoints.Sum(p => (p.Value - mean) * (p.Value - mean));
            result.Summary.StandardDeviation = Math.Round(
                    (decimal)Math.Sqrt((double)(sumOfSquaredDifferences / dataPoints.Count)), 2);

            // Calculate CMGR (Compound Monthly Growth Rate)
            // CMGR = (Last/First)^(1/n) - 1 where n is number of months
            if (firstValue > 0 && lastValue > 0)
            {
                // Calculate months between first and last data point
                var firstDate = dataPoints.First().Date;
                var lastDate = dataPoints.Last().Date;
                var months = ((lastDate.Year - firstDate.Year) * 12) + lastDate.Month - firstDate.Month;

                if (months > 0)
                {
                    var ratio = (double)(lastValue / firstValue);
                    var cmgr = (decimal)(Math.Pow(ratio, 1.0 / months) - 1);
                    result.Summary.CMGR = Math.Round(cmgr * 100, 2); // Convert to percentage
                }
            }
        }

        /// <summary>
        /// Helper class for time-based grouping
        /// </summary>
        private class TimeGroup
        {
            public DateTime Date { get; set; }
            public List<Dictionary<string, object>> Items { get; set; } = new List<Dictionary<string, object>>();
        }

        /// <summary>
        /// Group data by the specified time dimension
        /// </summary>
        private List<TimeGroup> GroupByTimeFunc(
            List<Dictionary<string, object>> data,
            string timeDimension)
        {
            // Dictionary to hold the time groups
            var timeGroups = new Dictionary<string, TimeGroup>();

            foreach (var item in data)
            {
                // Get the date from the item
                if (!item.TryGetValue("Date", out var dateObj) ||
                    !(dateObj is DateTime date))
                {
                    // Skip items without a valid date
                    continue;
                }

                // Get the key based on time dimension
                string key = GetTimeKey(date, timeDimension);
                DateTime groupDate = GetGroupDate(date, timeDimension);

                // Add to the appropriate group
                if (!timeGroups.TryGetValue(key, out var group))
                {
                    group = new TimeGroup { Date = groupDate };
                    timeGroups[key] = group;
                }

                group.Items.Add(item);
            }

            return timeGroups.Values.ToList();
        }

        /// <summary>
        /// Get a key for time grouping based on time dimension
        /// </summary>
        private string GetTimeKey(DateTime date, string timeDimension)
        {
            return timeDimension.ToLowerInvariant() switch
            {
                "daily" => date.ToString("yyyy-MM-dd"),
                "weekly" => $"{date.Year}-W{GetWeekNumber(date)}",
                "monthly" => date.ToString("yyyy-MM"),
                "quarterly" => $"{date.Year}-Q{(date.Month - 1) / 3 + 1}",
                "yearly" => date.ToString("yyyy"),
                _ => date.ToString("yyyy-MM-dd") // Default to daily
            };
        }

        /// <summary>
        /// Get a representative date for the time group
        /// </summary>
        private DateTime GetGroupDate(DateTime date, string timeDimension)
        {
            return timeDimension.ToLowerInvariant() switch
            {
                "daily" => date.Date,
                "weekly" => GetFirstDayOfWeek(date),
                "monthly" => new DateTime(date.Year, date.Month, 1),
                "quarterly" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
                "yearly" => new DateTime(date.Year, 1, 1),
                _ => date.Date // Default to daily
            };
        }

        /// <summary>
        /// Get the ISO week number for a date
        /// </summary>
        private int GetWeekNumber(DateTime date)
        {
            var day = (int)System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date.AddDays(4 - (day == 0 ? 7 : day)),
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        /// <summary>
        /// Get the first day of the week containing the specified date
        /// </summary>
        private DateTime GetFirstDayOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Calculate overall trend from the data
        /// </summary>
        private void CalculateOverallTrend(List<Dictionary<string, object>> data, TrendResult result, TrendParameters parameters)
        {
            // Group data by time dimension
            var timeGroups = GroupByTimeFunc(data, parameters.TimeDimension);
            
            // Sort groups by date
            timeGroups.Sort((a, b) => a.Date.CompareTo(b.Date));
            
            // Calculate metric value for each time group
            var dataPoints = new List<TrendDataPoint>();
            foreach (var group in timeGroups)
            {
                var value = CalculateMetricValue(group.Items, parameters.Metric);
                dataPoints.Add(new TrendDataPoint
                {
                    Date = group.Date,
                    Value = value,
                    ItemCount = group.Items.Count
                });
            }
            
            result.DataPoints = dataPoints;
        }

        /// <summary>
        /// Calculate grouped trends based on group by field
        /// </summary>
        private void CalculateGroupedTrends(List<Dictionary<string, object>> data, TrendResult result, TrendParameters parameters)
        {
            // Group data by the specified field first
            var groups = data
                .GroupBy(item => item.TryGetValue(parameters.GroupBy, out var value) ? value?.ToString() : "Unknown")
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // For each group, calculate the time-based trend
            foreach (var group in groups)
            {
                if (string.IsNullOrEmpty(group.Key) || group.Key == "Unknown" || group.Value.Count == 0)
                {
                    continue;
                }
                
                // Get time groups for this group
                var timeGroups = GroupByTimeFunc(group.Value, parameters.TimeDimension);
                
                // Sort by date
                timeGroups.Sort((a, b) => a.Date.CompareTo(b.Date));
                
                // Calculate metric values
                var dataPoints = new List<TrendDataPoint>();
                foreach (var timeGroup in timeGroups)
                {
                    var value = CalculateMetricValue(timeGroup.Items, parameters.Metric);
                    dataPoints.Add(new TrendDataPoint
                    {
                        Date = timeGroup.Date,
                        Value = value,
                        ItemCount = timeGroup.Items.Count
                    });
                }
                
                // Add to grouped data
                result.GroupedData[group.Key] = dataPoints;
            }
        }

        /// <summary>
        /// Detect spikes in the data (sudden increases or decreases)
        /// </summary>
        private void DetectSpikes(List<TrendDataPoint> dataPoints, List<TrendPattern> trends)
        {
            if (dataPoints.Count < 3)
            {
                return;
            }
            
            // Calculate average change between consecutive points
            decimal sumAbsChange = 0;
            for (int i = 1; i < dataPoints.Count; i++)
            {
                sumAbsChange += Math.Abs(dataPoints[i].Value - dataPoints[i - 1].Value);
            }
            
            decimal avgChange = sumAbsChange / (dataPoints.Count - 1);
            
            // Define a threshold for significant change (e.g., 3x the average)
            decimal threshold = avgChange * 3;
            
            // Look for spikes
            for (int i = 1; i < dataPoints.Count; i++)
            {
                var prev = dataPoints[i - 1].Value;
                var curr = dataPoints[i].Value;
                var change = Math.Abs(curr - prev);
                
                // Check if the change exceeds our threshold
                if (change > threshold)
                {
                    string direction = curr > prev ? "up" : "down";
                    decimal percentChange = prev != 0 ? Math.Round(((curr - prev) / prev) * 100, 2) : 0;
                    
                    trends.Add(new TrendPattern
                    {
                        PatternType = $"Spike {direction}",
                        StartDate = dataPoints[i - 1].Date,
                        EndDate = dataPoints[i].Date,
                        Magnitude = percentChange,
                        Description = $"Sudden {direction}ward change of {percentChange}% from {dataPoints[i - 1].Date:d} to {dataPoints[i].Date:d}"
                    });
                }
            }
        }

        /// <summary>
        /// Detect plateaus (periods of stability) in the data
        /// </summary>
        private void DetectPlateaus(List<TrendDataPoint> dataPoints, List<TrendPattern> trends)
        {
            if (dataPoints.Count < 3)
            {
                return;
            }
            
            int plateauStartIndex = 0;
            bool inPlateau = false;
            decimal plateauValue = 0;
            
            // Calculate average value for reference
            decimal avgValue = dataPoints.Average(p => p.Value);
            
            // Define stability threshold (e.g., less than 5% change from plateau value)
            decimal stabilityThreshold = avgValue * 0.05m;
            
            for (int i = 1; i < dataPoints.Count; i++)
            {
                var curr = dataPoints[i].Value;
                
                if (!inPlateau)
                {
                    // Check if this could be the start of a plateau
                    var prev = dataPoints[i - 1].Value;
                    if (Math.Abs(curr - prev) <= stabilityThreshold)
                    {
                        inPlateau = true;
                        plateauStartIndex = i - 1;
                        plateauValue = (curr + prev) / 2; // Use average as the plateau value
                    }
                }
                else
                {
                    // Check if we're still in the plateau
                    if (Math.Abs(curr - plateauValue) > stabilityThreshold)
                    {
                        // End of plateau - check if it was long enough (at least 3 points)
                        if (i - plateauStartIndex >= 3)
                        {
                            trends.Add(new TrendPattern
                            {
                                PatternType = "Plateau",
                                StartDate = dataPoints[plateauStartIndex].Date,
                                EndDate = dataPoints[i - 1].Date,
                                Magnitude = Math.Round(plateauValue, 2),
                                Description = $"Stable period around {Math.Round(plateauValue, 2)} from {dataPoints[plateauStartIndex].Date:d} to {dataPoints[i - 1].Date:d}"
                            });
                        }
                        
                        inPlateau = false;
                    }
                }
            }
            
            // Check if we have an ongoing plateau at the end
            if (inPlateau && dataPoints.Count - plateauStartIndex >= 3)
            {
                trends.Add(new TrendPattern
                {
                    PatternType = "Plateau",
                    StartDate = dataPoints[plateauStartIndex].Date,
                    EndDate = dataPoints[dataPoints.Count - 1].Date,
                    Magnitude = Math.Round(plateauValue, 2),
                    Description = $"Stable period around {Math.Round(plateauValue, 2)} from {dataPoints[plateauStartIndex].Date:d} to {dataPoints[dataPoints.Count - 1].Date:d}"
                });
            }
        }
        
        /// <summary>
        /// Fetch entity data from the appropriate source
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchEntityDataAsync(
            string entityType,
            string startDate,
            string endDate,
            List<FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Pass through to the data fetch service
            var result = await _dataFetcher.FetchEntityDataAsync(
                entityType,
                null,  // No entity ID for trend data
                new Dictionary<string, string>
                {
                    { "startDate", startDate },
                    { "endDate", endDate }
                },
                cancellationToken);
                
            // Fix pattern matching by using explicit type checking and conversion
            if (result is Dictionary<string, object> dictData)
            {
                // If result is a single dictionary, wrap it in a list
                Console.WriteLine($"Processing dictionary data: {dictData.Count} entries.");
                return new List<Dictionary<string, object>> { dictData };
            }
            else if (data is List<Dictionary<string, object>> listData)
            {
                // Process listData
                Console.WriteLine($"Processing list data: {listData.Count} entries.");
                return listData;
            }
            else
            {
                // Return an empty list if result is null or unexpected type
                _logger.LogWarning($"Unexpected result type from FetchEntityDataAsync: {result?.GetType().Name ?? "null"}");
                return new List<Dictionary<string, object>>();
            }
        }

        /// <summary>
        /// Calculate a metric value for a collection of items
        /// </summary>
        private decimal CalculateMetricValue(
            List<Dictionary<string, object>> items,
            string metric)
        {
            // Use the metric calculation service
            var calculator = new MetricCalculationService();

            switch (metric.ToLowerInvariant())
            {
                case "totalhours":
                    return calculator.CalculateTotalHours(items);

                case "totalcost":
                    return calculator.CalculateTotalCost(items);

                case "averagehours":
                    return calculator.CalculateAverageHours(items);

                case "averagecost":
                    return calculator.CalculateAverageCost(items);

                case "totalrevenue":
                    return calculator.CalculateTotalRevenue(items);

                case "totalprofit":
                    return calculator.CalculateTotalProfit(items);

                case "billablehours":
                    return calculator.CalculateBillableHours(items);

                case "nonbillablehours":
                    return calculator.CalculateNonBillableHours(items);

                case "utilization":
                    return calculator.CalculateUtilizationRate(items) * 100; // Convert to percentage

                case "hourlycost":
                    return calculator.CalculateAverageHourlyRate(items);

                case "profitmargin":
                    return calculator.CalculateProfitMargin(items);

                case "count":
                    return items.Count;

                default:
                    throw new ArgumentException($"Unsupported metric: {metric}");
            }
        }
    }
}