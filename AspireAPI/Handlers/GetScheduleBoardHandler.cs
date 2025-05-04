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
    public class GetScheduleBoardHandler : BaseHandler
    {
        public GetScheduleBoardHandler(
            ILogger<GetScheduleBoardHandler> logger,
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
                var responseContent = await GetScheduleBoardFromApi(accessToken, cancellationToken);
                return CreateResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetScheduleBoardHandler");
                throw new McpServerException($"Error getting schedule board: {ex.Message}", ex);
            }
        }

        private async Task<string> GetScheduleBoardFromApi(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/ScheduleBoard";
            
            var response = await client.GetAsync(url, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}