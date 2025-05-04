using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Models;
using System.Text.Json;

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for formatting report output in various formats
    /// </summary>
    public class ReportOutputService
    {
        private readonly ILogger<ReportOutputService> _logger;

        public ReportOutputService(ILogger<ReportOutputService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Formats report data into the requested output format
        /// </summary>
        /// <param name="data">Report data</param>
        /// <param name="format">Output format</param>
        /// <param name="reportName">Name of the report</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Formatted report data</returns>
        public async Task<byte[]> FormatOutputAsync(
            List<Dictionary<string, object>> data,
            string format,
            string reportName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Formatting report {reportName} to {format}");
            
            // Simple implementation: convert everything to JSON for now
            var json = JsonSerializer.Serialize(data ?? new List<Dictionary<string, object>>());
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Determines the appropriate filename for the report
        /// </summary>
        /// <param name="reportName">Name of the report</param>
        /// <param name="format">Output format</param>
        /// <returns>A filename for the report</returns>
        public string GetReportFilename(string reportName, string format)
        {
            var safeName = string.IsNullOrWhiteSpace(reportName) 
                ? "report" 
                : reportName.Replace(" ", "_");
                
            return $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToLowerInvariant()}";
        }

        /// <summary>
        /// Gets the content type for the specified format
        /// </summary>
        /// <param name="format">Output format</param>
        /// <returns>The MIME content type</returns>
        public string GetContentType(string format)
        {
            return format?.ToLowerInvariant() switch
            {
                "json" => "application/json",
                "csv" => "text/csv",
                "pdf" => "application/pdf",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "html" => "text/html",
                _ => "application/octet-stream"
            };
        }
    }
}