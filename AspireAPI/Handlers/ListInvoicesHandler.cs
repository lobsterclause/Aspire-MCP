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
    public class ListInvoicesHandler : BaseHandler
    {
        public ListInvoicesHandler(
            ILogger<ListInvoicesHandler> logger,
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
                var responseContent = await GetInvoicesFromApi(accessToken, cancellationToken);
                return CreateResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListInvoicesHandler");
                throw new McpServerException($"Error listing invoices: {ex.Message}", ex);
            }
        }

        private async Task<string> GetInvoicesFromApi(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Invoices";
            
            var response = await client.GetAsync(url, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}