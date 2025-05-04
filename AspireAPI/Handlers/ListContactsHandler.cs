using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace AspireAPI.Handlers
{
    public class ListContactsHandler : BaseHandler
    {
        private readonly AspireApiService _apiService;

        public ListContactsHandler(
            ILogger<ListContactsHandler> logger,
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
                if (!arguments.TryGetValue("type", out var typeObj) || typeObj == null)
                {
                    throw new McpServerException("Type parameter is required (customer, vendor, employee, or all)");
                }

                string type = typeObj.ToString().ToLower();
                
                // Validate type parameter
                if (type != "customer" && type != "vendor" && type != "employee" && type != "all")
                {
                    throw new McpServerException("Invalid type. Must be one of: customer, vendor, employee, or all");
                }

                // Extract optional search parameter
                string search = null;
                if (arguments.TryGetValue("search", out var searchObj) && searchObj != null)
                {
                    search = searchObj.ToString();
                }

                // Call API service to get contacts
                var contacts = await _apiService.GetContactsAsync(
                    type, 
                    search, 
                    cancellationToken: cancellationToken);

                // Format response
                var response = new
                {
                    success = true,
                    contacts = contacts.Data,
                    total = contacts.Total,
                    pageNumber = contacts.PageNumber,
                    pageSize = contacts.PageSize
                };

                return CreateResponse(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListContactsHandler");
                throw new McpServerException($"Error listing contacts: {ex.Message}", ex);
            }
        }
    }
}