using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AspireAPI.Models; // Added using directive for Models
namespace AspireAPI.Handlers
{
    public class ListInventoryItemsHandler : BaseHandler
    {
        public ListInventoryItemsHandler(
            ILogger<ListInventoryItemsHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers)
            : base(logger, httpClientFactory, apiHelpers)
        {
        }

        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                var responseContent = await GetInventoryItemsFromApi(accessToken, cancellationToken);
                return CreateResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListInventoryItemsHandler");
                throw new McpServerException($"Error listing inventory items: {ex.Message}", ex);
            }
        }

        private async Task<string> GetInventoryItemsFromApi(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Inventory";
            
            var response = await client.GetAsync(url, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}