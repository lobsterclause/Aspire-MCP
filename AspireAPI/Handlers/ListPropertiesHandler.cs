using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using AspireAPI.Models;
using AspireAPI.Services;
using AspireAPI.ToolDefinitions;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace AspireAPI.Handlers
{
    public class ListPropertiesHandler : BaseHandler
    {
        private new readonly ILogger<ListPropertiesHandler> _logger;
        private readonly DataFetchService _dataFetchService;

        public ListPropertiesHandler(
            ILogger<ListPropertiesHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            DataFetchService dataFetchService) : base(logger, httpClientFactory, apiHelpers)
        {
            _logger = logger;
            _dataFetchService = dataFetchService;
        }

        // Implementation for BaseHandler requirement
        public override Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("ListProperties tool called but is not enabled in this minimal server version.");
            var response = new CallToolResponse().WithError("Tool 'ListProperties' is not available in this server configuration.");
            return Task.FromResult(response);
        }
        
        public async Task<string> HandleListPropertiesAsync(string arguments)
        {
            try
            {
                var input = JsonSerializer.Deserialize<ListPropertiesToolDefinition.ListPropertiesInput>(arguments);
                
                string endpoint = "properties";
                var queryParams = new Dictionary<string, string>();
                
                // Build OData query parameters
                if (!string.IsNullOrEmpty(input?.ODataQuery))
                {
                    queryParams.Add("$filter", input.ODataQuery);
                }
                
                // Add search query if provided
                if (!string.IsNullOrEmpty(input?.Query))
                {
                    queryParams.Add("$search", input.Query);
                }
                
                // Pagination parameters
                if (input?.PageSize.HasValue == true)
                {
                    queryParams.Add("$top", input.PageSize.Value.ToString());
                }
                
                if (input?.PageNumber.HasValue == true)
                {
                    int skip = (input.PageNumber.Value - 1) * (input.PageSize ?? 50);
                    queryParams.Add("$skip", skip.ToString());
                }
                
                // Handle related data inclusion
                if (input?.IncludeRelated == true)
                {
                    queryParams.Add("$expand", "Owner,Location,SalesAgent");
                }
                
                // Add standard selects for properties
                queryParams.Add("$select", "Id,PropertyName,Address,City,State,ZipCode,ListingPrice,Status,PropertyType,SquareFootage,Bedrooms,Bathrooms,YearBuilt,Description");
                
                // Fetch data using the DataFetchService
                var result = await _dataFetchService.FetchDataWithQueryAsync(endpoint, queryParams);
                
                if (result.IsError)
                {
                    _logger.LogError("Error fetching properties: {ErrorMessage}", result.ErrorMessage);
                    return JsonSerializer.Serialize(new { error = result.ErrorMessage });
                }
                
                // Process the response
                var searchResult = new SearchResult
                {
                    Query = input?.Query,
                    EntityType = "Property",
                    Results = (List<SearchResult>)result.Data,
                    TotalCount = result.TotalCount,
                    PageSize = input?.PageSize ?? 50,
                    PageNumber = input?.PageNumber ?? 1
                };
                
                return JsonSerializer.Serialize(searchResult, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing properties request");
                return JsonSerializer.Serialize(new { error = $"Failed to process properties request: {ex.Message}" });
            }
        }
    }
}