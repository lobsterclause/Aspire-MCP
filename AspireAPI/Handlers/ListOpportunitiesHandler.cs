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
    public class ListOpportunitiesHandler : BaseHandler
    {
        public ListOpportunitiesHandler(
            ILogger<ListOpportunitiesHandler> logger,
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
                string clientId = await GetClientIdFromArguments(arguments, accessToken, cancellationToken);
                var responseContent = await GetOpportunitiesFromApi(clientId, accessToken, cancellationToken);
                return CreateResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListOpportunitiesHandler");
                throw new McpServerException($"Error listing opportunities: {ex.Message}", ex);
            }
        }

        private async Task<string> GetClientIdFromArguments(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            // First check if clientId is provided directly
            if (arguments.TryGetValue("clientId", out var clientIdObj) && clientIdObj != null)
            {
                return clientIdObj.ToString();
            }
            
            // If not, check if clientName is provided and resolve it to an ID
            // Fixed: Removed accessToken parameter which isn't in the method signature
            if (arguments.TryGetValue("clientName", out var clientNameObj) && clientNameObj != null)
            {
                return await _apiHelpers.GetClientIdByNameAsync(clientNameObj.ToString(), cancellationToken);
            }
            
            return null;
        }

        private async Task<string> GetOpportunitiesFromApi(
            string clientId,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Opportunities";
            
            if (!string.IsNullOrEmpty(clientId))
            {
                url += $"?clientId={Uri.EscapeDataString(clientId)}";
            }

            var response = await client.GetAsync(url, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}