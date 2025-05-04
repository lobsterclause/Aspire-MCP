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
    /// Service for converting data to different export formats (JSON, CSV, Excel)
    /// </summary>
    public class DataConverterService
    {
        private readonly ILogger<DataConverterService> _logger; // Use specific logger type

        public DataConverterService(ILogger<DataConverterService> logger) // Use specific logger type
        {
            _logger = logger;
        }

        /// <summary>
        /// Converts a list of dictionaries to JSON format
        /// </summary>
        public (byte[] Data, string ContentType) FormatAsJson(List<Dictionary<string, object>> data)
        {
            var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            return (bytes, "application/json");
        }

        /// <summary>
        /// Converts a list of dictionaries to CSV format
        /// </summary>
        public (byte[] Data, string ContentType) FormatAsCsv(
            List<Dictionary<string, object>> data,
            bool includeHeaders,
            string delimiter)
        {
            var csvBuilder = new StringBuilder();
            delimiter ??= ","; // Default delimiter is comma

            if (includeHeaders && data.Any())
            {
                // Ensure consistent header order based on the first item
                var headers = data.First().Keys.ToList();
                csvBuilder.AppendLine(string.Join(delimiter, headers.Select(h => EscapeCsvField(h, delimiter))));

                foreach (var item in data)
                {
                    // Output values in the same order as headers
                    var values = headers.Select(header => item.TryGetValue(header, out var value) ? EscapeCsvField(value?.ToString(), delimiter) : "");
                    csvBuilder.AppendLine(string.Join(delimiter, values));
                }
            }
            else // Handle case with no headers or no data
            {
                 foreach (var item in data)
                 {
                     csvBuilder.AppendLine(string.Join(delimiter, item.Values.Select(v => EscapeCsvField(v?.ToString(), delimiter))));
                 }
            }


            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return (bytes, "text/csv");
        }

        /// <summary>
        /// Escapes a field for CSV format
        /// </summary>
        private string EscapeCsvField(string field, string delimiter)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }

            // Fields containing delimiter, double quote, or newline must be quoted
            if (field.Contains(delimiter) || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Double up double quotes within the field
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Converts a list of dictionaries to Excel (XLSX) format (Placeholder)
        /// </summary>
        public (byte[] Data, string ContentType) FormatAsExcel(
            List<Dictionary<string, object>> data,
            bool includeHeaders)
        {
            // This is a placeholder. Generating XLSX requires a library like EPPlus.
            // For now, return a simple message or throw an exception.
            _logger.LogWarning("Excel export is not fully implemented.");
            var message = "Excel export is not supported in this version.";
            var bytes = Encoding.UTF8.GetBytes(message);
            // Consider throwing NotImplementedException if preferred
            // throw new NotImplementedException("Excel export not implemented");
            return (bytes, "text/plain");
        }
    }
}