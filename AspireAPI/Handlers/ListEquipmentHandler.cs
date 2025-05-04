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
    public class ListEquipmentHandler : BaseHandler
    {
        private new readonly ILogger<ListEquipmentHandler> _logger;
        private readonly DataFetchService _dataFetchService;

        public ListEquipmentHandler(
            ILogger<ListEquipmentHandler> logger,
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
            _logger.LogWarning("ListEquipment tool called but is not enabled in this minimal server version.");
            var response = new CallToolResponse().WithError("Tool 'ListEquipment' is not available in this server configuration.");
            return Task.FromResult(response);
        }
        
        public async Task<string> HandleListEquipmentAsync(string arguments)
        {
            try
            {
                var input = JsonSerializer.Deserialize<ListEquipmentToolDefinition.ListEquipmentInput>(arguments);
                
                string endpoint = "equipment";
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
                    queryParams.Add("$expand", "EquipmentClass,Model,Manufacturer");
                }
                
                // Add standard selects for equipment
                queryParams.Add("$select", "Id,EquipmentName,SerialNumber,Status,AcquisitionDate,ModelNumber,Manufacturer,EquipmentClass,PurchasePrice,CurrentValue,Location,LastMaintenanceDate,NextMaintenanceDate");
                
                // Fetch data using the DataFetchService
                var result = await _dataFetchService.FetchDataWithQueryAsync(endpoint, queryParams);
                
                if (result.IsError)
                {
                    _logger.LogError("Error fetching equipment: {ErrorMessage}", result.ErrorMessage);
                    return JsonSerializer.Serialize(new { error = result.ErrorMessage });
                }
                
                // Process the response
                var searchResult = new SearchResult
                {
                    Query = input?.Query,
                    EntityType = "Equipment",
                    Results = new List<SearchResult>(),
                    TotalCount = result.TotalCount,
                    PageSize = input?.PageSize ?? 50,
                    PageNumber = input?.PageNumber ?? 1
                };

                // Properly handle the dynamic data conversion
                if (result.Data is List<dynamic> dynamicList)
                {
                    foreach (var item in dynamicList)
                    {
                        var resultItem = new SearchResult
                        {
                            Id = item.Id != null ? Convert.ToString(item.Id) : "",
                            Title = item.EquipmentName != null ? Convert.ToString(item.EquipmentName) : "",
                            EntityType = "Equipment"
                        };
                        searchResult.Results.Add(resultItem);
                    }
                }
                
                return JsonSerializer.Serialize(searchResult, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing equipment request");
                return JsonSerializer.Serialize(new { error = $"Failed to process equipment request: {ex.Message}" });
            }
        }
    }
}