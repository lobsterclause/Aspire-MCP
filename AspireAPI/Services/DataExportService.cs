using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Export; // Assuming Export models are in AspireAPI.Export
using AspireAPI.Models; // Assuming models are in AspireAPI.Models

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for exporting data from Aspire
    /// </summary>
    public class DataExportService
    {
        private readonly AspireApiService _aspireApi;
        private readonly ILogger<DataExportService> _logger; // Use specific logger type
        private readonly DataConverterService _converterService;
        private readonly DataFetchService _dataFetchService; // Inject DataFetchService

        public DataExportService(
            AspireApiService aspireApi, 
            ILogger<DataExportService> logger, // Use specific logger type
            DataConverterService converterService, // Inject DataConverterService
            DataFetchService dataFetchService) // Inject DataFetchService
        {
            _aspireApi = aspireApi;
            _logger = logger;
            _converterService = converterService; // Assign injected service
            _dataFetchService = dataFetchService; // Assign injected service
        }

        /// <summary>
        /// Export data according to the specified parameters
        /// </summary>
        public async Task<ExportResult> ExportDataAsync(
            ExportParameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                // Fetch data from Aspire API
                var parameters_dict = new Dictionary<string, string>
                {
                    { "startDate", parameters.StartDate },
                    { "endDate", parameters.EndDate }
                };
                
                var data = await _dataFetchService.FetchEntityDataAsync(
                    parameters.EntityType,
                    null, // No specific entity ID
                    parameters_dict,
                    cancellationToken);
                
                // Convert single dictionary result to list if needed
                List<Dictionary<string, object>> dataList;
                if (data == null)
                {
                    dataList = new List<Dictionary<string, object>>();
                }
                else
                {
                    dataList = new List<Dictionary<string, object>> { data };
                }

                // Generate the export in the requested format
                var (exportData, contentType) = FormatDataForExport(
                    data, 
                    parameters.Format, 
                    parameters.IncludeHeaders, 
                    parameters.Delimiter);

                // Create the file name with appropriate extension
                string fileNameWithExt = $"{parameters.FileName}.{GetFileExtension(parameters.Format)}";

                // Create and return the export result
                return new ExportResult
                {
                    FileName = fileNameWithExt,
                    ContentType = contentType,
                    FileSize = exportData.Length,
                    Data = Convert.ToBase64String(exportData),
                    EntityType = parameters.EntityType,
                    RecordCount = data.Count(),
                    Format = parameters.Format,
                    ExportedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting {parameters.EntityType} data");
                throw;
            }
        }

        /// <summary>
        /// Format data in the requested export format
        /// </summary>
        private (byte[] Data, string ContentType) FormatDataForExport(
            List<Dictionary<string, object>> data,
            string format,
            bool includeHeaders,
            string delimiter)
        {
            switch (format.ToLowerInvariant())
            {
                case "json":
                    return _converterService.FormatAsJson(data);
                    
                case "csv":
                    return _converterService.FormatAsCsv(data, includeHeaders, delimiter);
                    
                case "excel":
                    return _converterService.FormatAsExcel(data, includeHeaders);
                    
                default:
                    throw new InvalidOperationException($"Unsupported format: {format}");
            }
        }

        /// <summary>
        /// Get file extension based on format
        /// </summary>
        private string GetFileExtension(string format)
        {
            return format.ToLowerInvariant() switch
            {
                "json" => "json",
                "csv" => "csv",
                "excel" => "xlsx",
                _ => "txt"
            };
        }
    }
}

// Assuming these models are defined elsewhere or need to be created
namespace AspireAPI.Export
{
    public class ExportParameters
    {
        public string EntityType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<FilterCondition> Filters { get; set; }
        public string Format { get; set; }
        public string FileName { get; set; }
        public bool IncludeHeaders { get; set; }
        public string Delimiter { get; set; }
    }

    public class ExportResult
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Data { get; set; } // Base64 encoded data
        public string EntityType { get; set; }
        public int RecordCount { get; set; }
        public string Format { get; set; }
        public DateTime ExportedAt { get; set; }
    }
}

// Assuming FilterCondition is defined in AspireAPI.Models or elsewhere
// using AspireAPI.Models;