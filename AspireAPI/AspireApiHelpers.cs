using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Models;

namespace AspireAPI
{
    /// <summary>
    /// Helper methods for interacting with the Aspire API
    /// </summary>
    public class AspireApiHelpers
    {
        private readonly ILogger<AspireApiHelpers> _logger;

        public AspireApiHelpers(ILogger<AspireApiHelpers> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Builds a query string from parameters
        /// </summary>
        /// <param name="parameters">Dictionary of parameter name/value pairs</param>
        /// <returns>Formatted query string</returns>
        public string BuildQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            foreach (var param in parameters)
            {
                if (!string.IsNullOrEmpty(param.Value))
                {
                    parts.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
                }
            }

            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }

        /// <summary>
        /// Extracts response content as a specific type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="response">HTTP response message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized object of type T</returns>
        public async Task<T> ExtractResponseContentAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            try
            {
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response");
                throw new Exception("Failed to parse API response", ex);
            }
        }

        /// <summary>
        /// Formats a date in the format expected by the API
        /// </summary>
        /// <param name="date">The date to format</param>
        /// <returns>Formatted date string</returns>
        public string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Formats a date range for API queries
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>Formatted date range string</returns>
        public string FormatDateRange(DateTime start, DateTime end)
        {
            return $"{FormatDate(start)} to {FormatDate(end)}";
        }
        
        /// <summary>
        /// Gets a client ID by name
        /// </summary>
        /// <param name="clientName">The name of the client to look up</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Client ID if found</returns>
        public async Task<string> GetClientIdByNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(clientName));
            }
            
            // In a real implementation, this would query the Aspire API
            // For now, implement a basic version that works for testing
            _logger.LogInformation($"Looking up client ID for client name: {clientName}");
            
            // Mock implementation for testing purposes
            // In a real implementation, this would call the Aspire API to search for clients
            // and return the actual client ID for the given name
            return $"client-{clientName.ToLowerInvariant().Replace(" ", "-")}";
        }
    }
}