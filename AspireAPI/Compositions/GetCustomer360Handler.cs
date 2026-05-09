using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Compositions
{
    /// <summary>
    /// Returns a single customer-centric payload: contact + properties they own +
    /// opportunities + invoices + payment history. One call instead of 5.
    /// </summary>
    public sealed class GetCustomer360Handler : CompositionBase
    {
        public GetCustomer360Handler(
            ILogger<GetCustomer360Handler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client,
            TokenService tokenService)
            : base(logger, httpClientFactory, apiHelpers, client, tokenService) { }

        protected override async Task<JsonNode> ComposeAsync(
            IReadOnlyDictionary<string, object?> args,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var contactId = Required(args, "contactId");
            var filter = $"ContactID eq {contactId}";

            var contactTask = SafeAsync(() => ListWithFilterAsync("Contacts", $"ContactID eq {contactId}", accessToken, cancellationToken));
            var propertiesTask = SafeAsync(() => ListWithFilterAsync("Properties", filter, accessToken, cancellationToken));
            var opportunitiesTask = SafeAsync(() => ListWithFilterAsync("Opportunities", filter, accessToken, cancellationToken));
            var invoicesTask = SafeAsync(() => ListWithFilterAsync("Invoices", filter, accessToken, cancellationToken));
            var paymentsTask = SafeAsync(() => ListWithFilterAsync("Payments", filter, accessToken, cancellationToken));

            await Task.WhenAll(contactTask, propertiesTask, opportunitiesTask, invoicesTask, paymentsTask);

            return new JsonObject
            {
                ["contactId"] = contactId,
                ["contact"] = contactTask.Result,
                ["properties"] = propertiesTask.Result,
                ["opportunities"] = opportunitiesTask.Result,
                ["invoices"] = invoicesTask.Result,
                ["payments"] = paymentsTask.Result,
            };
        }
    }
}
