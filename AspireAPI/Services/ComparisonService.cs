using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Comparison;
using AspireAPI.Services;
using AspireAPI.Models; // Added using directive for Models

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for comparing data across different dimensions
    /// </summary>
    public class ComparisonService
    {
        private readonly AspireApiService _aspireApi;
        private readonly ILogger _logger;
        private readonly DataFetchService _dataFetcher;
        private readonly DateRangeService _dateRangeService;
        private readonly MetricCalculationService _metricCalculator; // Added MetricCalculationService

        public ComparisonService(
            AspireApiService aspireApi,
            ILogger<ComparisonService> logger, // Corrected logger type
            DataFetchService dataFetcher,
            DateRangeService dateRangeService,
            MetricCalculationService metricCalculator) // Inject dependencies
        {
            _aspireApi = aspireApi;
            _logger = logger;
            _dataFetcher = dataFetcher; // Use injected service
            _dateRangeService = dateRangeService; // Use injected service
            _metricCalculator = metricCalculator; // Use injected service
        }

        /// <summary>
        /// Compare data according to the specified parameters
        /// </summary>
        public async Task<ComparisonResult> CompareDataAsync(
            ComparisonParameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                // Fetch first dataset
                var firstData = await FetchComparisonDataAsync(
                    parameters.EntityType,
                    parameters.Dimension,
                    parameters.FirstValue,
                    parameters.FirstStartDate,
                    parameters.FirstEndDate,
                    parameters.Filters,
                    cancellationToken);

                // Fetch second dataset
                var secondData = await FetchComparisonDataAsync(
                    parameters.EntityType,
                    parameters.Dimension,
                    parameters.SecondValue,
                    parameters.SecondStartDate,
                    parameters.SecondEndDate,
                    parameters.Filters,
                    cancellationToken);

                // Calculate metrics for both datasets
                var firstMetrics = CalculateMetrics(firstData, parameters.Metrics, parameters.GroupBy);
                var secondMetrics = CalculateMetrics(secondData, parameters.Metrics, parameters.GroupBy);

                // Compare the metrics and generate detailed results
                var comparisonDetails = CompareMetrics(firstMetrics, secondMetrics, parameters.Metrics);

                // Generate summary statistics
                var summary = GenerateComparisonSummary(firstMetrics, secondMetrics, parameters.Metrics);

                // Return the comparison result
                return new ComparisonResult
                {
                    EntityType = parameters.EntityType,
                    Dimension = parameters.Dimension,
                    FirstValue = parameters.FirstValue,
                    SecondValue = parameters.SecondValue,
                    Metrics = parameters.Metrics,
                    Summary = summary,
                    Details = comparisonDetails,
                    ComparedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error comparing {parameters.EntityType} data");
                throw;
            }
        }

        /// <summary>
        /// Fetch data for comparison based on dimension and value
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchComparisonDataAsync(
            string entityType,
            string dimension,
            string value,
            string startDate,
            string endDate,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Ensure filters list is not null
            if (filters == null)
            {
                filters = new List<AspireAPI.Models.FilterCondition>();
            }

            switch (dimension.ToLowerInvariant())
            {
                case "time":
                    return await FetchDataByTimeAsync(
                        entityType, value, startDate, endDate, filters, cancellationToken);

                case "branch":
                    return await FetchDataByBranchAsync(
                        entityType, value, filters, cancellationToken);

                case "division":
                    return await FetchDataByDivisionAsync(
                        entityType, value, filters, cancellationToken);

                case "employee":
                    return await FetchDataByEmployeeAsync(
                        entityType, value, filters, cancellationToken);

                case "client":
                    return await FetchDataByClientAsync(
                        entityType, value, filters, cancellationToken);

                default:
                    throw new ArgumentException($"Unsupported comparison dimension: {dimension}");
            }
        }

        /// <summary>
        /// Fetch data for a specific time period
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchDataByTimeAsync(
            string entityType,
            string timeFrame,
            string customStartDate,
            string customEndDate,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            string startDate, endDate;

            if (timeFrame.ToLowerInvariant() == "custom")
            {
                // Use custom date range
                if (string.IsNullOrEmpty(customStartDate) || string.IsNullOrEmpty(customEndDate))
                {
                    throw new ArgumentException("Custom time frame requires start and end dates");
                }

                startDate = customStartDate;
                endDate = customEndDate;
            }
            else
            {
                // Calculate standard date range
                var dateRange = _dateRangeService.GetDateRangeFromParameters(
                    new Dictionary<string, string> { { "dateRangeType", timeFrame } },
                    "startDate",
                    "endDate",
                    cancellationToken);

                startDate = dateRange.Start;
                endDate = dateRange.End;
            }

            // Fetch data for the specified time period
            return await _dataFetcher.FetchEntityDataAsync(
                entityType,
                startDate,
                endDate,
                filters,
                cancellationToken);
        }

        /// <summary>
        /// Fetch data for a specific branch
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchDataByBranchAsync(
            string entityType,
            string branchId,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Add branch filter
            var branchFilters = new List<AspireAPI.Models.FilterCondition>(filters)
            {
                new AspireAPI.Models.FilterCondition
                {
                    Field = "BranchId", // Assuming standard field name
                    Operator = "eq",
                    Value = branchId
                }
            };

            // Use current month as default time range (or consider making this configurable)
            var dateRange = _dateRangeService.GetDateRangeFromParameters(
                new Dictionary<string, string> { { "dateRangeType", "thisMonth" } },
                "startDate",
                "endDate",
                cancellationToken);

            // Fetch data with the branch filter
            return await _dataFetcher.FetchEntityDataAsync(
                entityType,
                dateRange.Start.ToString("yyyy-MM-dd"),
                dateRange.End.ToString("yyyy-MM-dd"),
                branchFilters,
                cancellationToken);
        }

        /// <summary>
        /// Fetch data for a specific division
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchDataByDivisionAsync(
            string entityType,
            string divisionId,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Add division filter
            var divisionFilters = new List<AspireAPI.Models.FilterCondition>(filters)
            {
                new AspireAPI.Models.FilterCondition
                {
                    Field = "DivisionId", // Assuming standard field name
                    Operator = "eq",
                    Value = divisionId
                }
            };

            // Use current month as default time range
            var dateRange = _dateRangeService.GetDateRangeFromParameters(
                new Dictionary<string, string> { { "dateRangeType", "thisMonth" } },
                "startDate",
                "endDate",
                cancellationToken);

            // Fetch data with the division filter
            return await _dataFetcher.FetchEntityDataAsync(
                entityType,
                dateRange.Start.ToString("yyyy-MM-dd"),
                dateRange.End.ToString("yyyy-MM-dd"),
                divisionFilters,
                cancellationToken);
        }

        /// <summary>
        /// Fetch data for a specific employee
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchDataByEmployeeAsync(
            string entityType,
            string employeeId,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Add employee filter
            var employeeFilters = new List<AspireAPI.Models.FilterCondition>(filters)
            {
                new AspireAPI.Models.FilterCondition
                {
                    Field = "EmployeeId", // Assuming standard field name
                    Operator = "eq",
                    Value = employeeId
                }
            };

            // Use current month as default time range
            var dateRange = _dateRangeService.GetDateRangeFromParameters(
                new Dictionary<string, string> { { "dateRangeType", "thisMonth" } },
                "startDate",
                "endDate",
                cancellationToken);

            // Fetch data with the employee filter
            return await _dataFetcher.FetchEntityDataAsync(
                entityType,
                dateRange.Start.ToString("yyyy-MM-dd"),
                dateRange.End.ToString("yyyy-MM-dd"),
                employeeFilters,
                cancellationToken);
        }

        /// <summary>
        /// Fetch data for a specific client
        /// </summary>
        private async Task<List<Dictionary<string, object>>> FetchDataByClientAsync(
            string entityType,
            string clientId,
            List<AspireAPI.Models.FilterCondition> filters,
            CancellationToken cancellationToken)
        {
            // Add client filter
            var clientFilters = new List<AspireAPI.Models.FilterCondition>(filters)
            {
                new AspireAPI.Models.FilterCondition
                {
                    Field = "ContactId", // Assuming standard field name for client ID
                    Operator = "eq",
                    Value = clientId
                }
            };

            // Use current month as default time range
            var dateRange = _dateRangeService.GetDateRangeFromParameters(
                new Dictionary<string, string> { { "dateRangeType", "thisMonth" } },
                "startDate",
                "endDate",
                cancellationToken);

            // Fetch data with the client filter
            return await _dataFetcher.FetchEntityDataAsync(
                entityType,
                dateRange.Start.ToString("yyyy-MM-dd"),
                dateRange.End.ToString("yyyy-MM-dd"),
                clientFilters,
                cancellationToken);
        }

        // --- Placeholder methods for metric calculation and comparison ---
        // These should ideally be implemented in separate services or helpers

        private Dictionary<string, decimal> CalculateMetrics(
            List<Dictionary<string, object>> data,
            List<string> metrics,
            string groupBy)
        {
            // Placeholder: Implement actual metric calculation using MetricCalculationService
            var calculatedMetrics = new Dictionary<string, decimal>();
            foreach (var metric in metrics)
            {
                calculatedMetrics[metric] = data.Count > 0 ? data.Count * 10m : 0m; // Dummy calculation
            }
            return calculatedMetrics;
        }

        private List<ComparisonDetail> CompareMetrics(
            Dictionary<string, decimal> firstMetrics,
            Dictionary<string, decimal> secondMetrics,
            List<string> metrics)
        {
            // Placeholder: Implement actual metric comparison
            var details = new List<ComparisonDetail>();
            foreach (var metric in metrics)
            {
                var firstValue = firstMetrics.GetValueOrDefault(metric, 0m);
                var secondValue = secondMetrics.GetValueOrDefault(metric, 0m);
                details.Add(new ComparisonDetail
                {
                    Metric = metric,
                    FirstValue = firstValue,
                    SecondValue = secondValue,
                    Difference = secondValue - firstValue,
                    PercentageChange = firstValue != 0 ? Math.Round(((secondValue - firstValue) / firstValue) * 100, 2) : (secondValue == 0 ? 0 : 100)
                });
            }
            return details;
        }

        private ComparisonSummary GenerateComparisonSummary(
            Dictionary<string, decimal> firstMetrics,
            Dictionary<string, decimal> secondMetrics,
            List<string> metrics)
        {
            // Placeholder: Implement actual summary generation
            return new ComparisonSummary
            {
                OverallTrend = "Stable", // Dummy summary
                KeyDifferences = metrics.Take(1).ToList() // Dummy key difference
            };
        }
    }
}