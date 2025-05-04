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
    public class ListDivisionsHandler : BaseHandler
    {
        public ListDivisionsHandler(
            ILogger<ListDivisionsHandler> logger,
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
                string searchTerm = ExtractSearchTerm(arguments);
                var responseContent = await GetDivisionsFromApi(searchTerm, accessToken, cancellationToken);
                return CreateResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListDivisionsHandler");
                throw new McpServerException($"Error listing divisions: {ex.Message}", ex);
            }
        }

        private string ExtractSearchTerm(IDictionary<string, object> arguments)
        {
            if (arguments.TryGetValue("search", out var searchObj) && searchObj != null)
            {
                return searchObj.ToString();
            }
            return null;
        }

        private async Task<string> GetDivisionsFromApi(
            string searchTerm, 
            string accessToken, 
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Divisions";
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                url += $"?search={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await client.GetAsync(url, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}