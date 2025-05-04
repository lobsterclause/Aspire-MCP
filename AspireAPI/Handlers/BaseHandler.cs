using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Models; // Added using directive for Models
using ModelContextProtocol.Protocol.Types;

namespace AspireAPI.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly ILogger _logger;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly AspireApiHelpers _apiHelpers;

        protected BaseHandler(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _apiHelpers = apiHelpers;
        }

        public abstract Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken);

        protected HttpClient CreateAuthenticatedClient(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return client;
        }

        protected CallToolResponse CreateResponse(string content)
        {
            return new CallToolResponse().WithContent(new Dictionary<string, object>
            {
                { "text", content }
            });
        }

        protected async Task<string> GetResponseContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error calling Aspire API: {response.StatusCode}");
            }
            
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}