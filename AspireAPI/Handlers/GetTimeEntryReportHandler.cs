using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using AspireAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.Handlers
{
    public class GetTimeEntryReportHandler : BaseHandler
    {
        private readonly AspireApiService _apiService;

        public GetTimeEntryReportHandler(
            ILogger<GetTimeEntryReportHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireApiService apiService)
            : base(logger, httpClientFactory, apiHelpers)
        {
            _apiService = apiService;
        }

        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                // Extract and validate required parameters
                if (!arguments.TryGetValue("clientName", out var clientNameObj) || clientNameObj == null)
                {
                    throw new McpServerException("clientName parameter is required");
                }

                if (!arguments.TryGetValue("dateRange", out var dateRangeObj) || dateRangeObj == null)
                {
                    throw new McpServerException("dateRange parameter is required");
                }

                string clientName = clientNameObj.ToString();
                string dateRange = dateRangeObj.ToString().ToLowerInvariant();

                // Extract optional division name
                string divisionName = null;
                if (arguments.TryGetValue("divisionName", out var divisionNameObj) && divisionNameObj != null)
                {
                    divisionName = divisionNameObj.ToString();
                }

                // Handle date range
                DateTime startDate;
                DateTime endDate;

                if (dateRange == "custom")
                {
                    // For custom range, extract start and end dates
                    if (!arguments.TryGetValue("startDate", out var startDateObj) || startDateObj == null)
                    {
                        throw new McpServerException("startDate parameter is required for custom date range");
                    }

                    if (!arguments.TryGetValue("endDate", out var endDateObj) || endDateObj == null)
                    {
                        throw new McpServerException("endDate parameter is required for custom date range");
                    }

                    if (!DateTime.TryParse(startDateObj.ToString(), out startDate))
                    {
                        throw new McpServerException("Invalid startDate format. Use yyyy-MM-dd format.");
                    }

                    if (!DateTime.TryParse(endDateObj.ToString(), out endDate))
                    {
                        throw new McpServerException("Invalid endDate format. Use yyyy-MM-dd format.");
                    }
                }
                else
                {
                    // Use DateRangeResolver for standard ranges
                    try
                    {
                        var dateRangeInput = new DateRangeQuery { Type = dateRange };
                        var dateRangeResult = DateRangeResolver.ResolveDateRange(dateRangeInput);
                        startDate = dateRangeResult.Start;
                        endDate = dateRangeResult.End;
                    }
                    catch (ArgumentException ex)
                    {
                        throw new McpServerException($"Invalid dateRange: {ex.Message}");
                    }
                }

                // Get contact and division IDs if needed
                string contactId = null;
                string divisionId = null;

                try
                {
                    contactId = await _apiService.GetContactIdByNameAsync("customer", clientName, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new McpServerException($"Error finding client '{clientName}': {ex.Message}");
                }

                if (!string.IsNullOrEmpty(divisionName))
                {
                    try
                    {
                        divisionId = await _apiService.GetDivisionIdByNameAsync(divisionName, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new McpServerException($"Error finding division '{divisionName}': {ex.Message}");
                    }
                }

                // Get time entries
                var timeEntries = await _apiService.GetTimeEntriesAsync(
                    startDate.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"),
                    contactId,
                    divisionId,
                    cancellationToken: cancellationToken);

                // Format response as a report
                var report = GenerateReport(timeEntries, startDate, endDate, clientName, divisionName);
                
                return CreateResponse(JsonSerializer.Serialize(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTimeEntryReportHandler");
                throw new McpServerException($"Error generating time entry report: {ex.Message}", ex);
            }
        }

        private object GenerateReport(List<TimeEntryDto> timeEntries, DateTime startDate, DateTime endDate, string clientName, string divisionName)
        {
            // Group time entries by date and then by employee
            var entriesByDate = timeEntries
                .GroupBy(te => te.Date.Date)
                .OrderBy(g => g.Key)
                .Select(dateGroup => new
                {
                    date = dateGroup.Key.ToString("yyyy-MM-dd"),
                    totalHours = dateGroup.Sum(e => e.Hours),
                    employees = dateGroup
                        .GroupBy(e => e.EmployeeName)
                        .OrderBy(g => g.Key)
                        .Select(employeeGroup => new
                        {
                            name = employeeGroup.Key,
                            hours = employeeGroup.Sum(e => e.Hours),
                            entries = employeeGroup.Select(e => new
                            {
                                id = e.Id,
                                description = e.Description,
                                hours = e.Hours,
                                jobName = e.JobName,
                                divisionName = e.DivisionName
                            }).ToList()
                        }).ToList()
                }).ToList();

            // Calculate summary statistics
            double totalHours = (double)timeEntries.Sum(e => e.Hours);
            
            var employeeSummary = timeEntries
                .GroupBy(e => e.EmployeeName)
                .OrderByDescending(g => g.Sum(e => e.Hours))
                .Select(g => new
                {
                    name = g.Key,
                    totalHours = g.Sum(e => e.Hours),
                    percentage = totalHours > 0 ? Math.Round(((double)g.Sum(e => e.Hours) / totalHours) * 100, 1) : 0
                }).ToList();

            // Build the final report
            return new
            {
                success = true,
                report = new
                {
                    clientName = clientName,
                    divisionName = divisionName,
                    dateRange = new
                    {
                        start = startDate.ToString("yyyy-MM-dd"),
                        end = endDate.ToString("yyyy-MM-dd"),
                        days = (endDate - startDate).Days + 1
                    },
                    summary = new
                    {
                        totalHours = totalHours,
                        totalEntries = timeEntries.Count,
                        averageHoursPerDay = entriesByDate.Count > 0 ? Math.Round((double)totalHours / entriesByDate.Count, 1) : 0,
                        employees = employeeSummary
                    },
                    dailyEntries = entriesByDate
                }
            };
        }
    }
}