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
    /// Returns a single timeline-shaped payload for one job: the job record itself
    /// plus every related opportunity, work ticket, invoice, and payment, stitched
    /// together. One composed call replaces the 5+ tools/list / tools/call round-trips
    /// the LLM would otherwise need.
    /// </summary>
    public sealed class GetJobLifecycleHandler : CompositionBase
    {
        public GetJobLifecycleHandler(
            ILogger<GetJobLifecycleHandler> logger,
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
            var jobId = Required(args, "jobId");
            var filter = $"JobID eq {jobId}";

            // Fan out in parallel — Aspire endpoints are independent, so we can
            // pipeline 5 GETs at the cost of 1 round-trip's worth of wall time.
            var jobTask = SafeAsync(() => ListWithFilterAsync("Jobs", $"JobID eq {jobId}", accessToken, cancellationToken));
            var opportunitiesTask = SafeAsync(() => ListWithFilterAsync("Opportunities", filter, accessToken, cancellationToken));
            var ticketsTask = SafeAsync(() => ListWithFilterAsync("WorkTickets", filter, accessToken, cancellationToken));
            var invoicesTask = SafeAsync(() => ListWithFilterAsync("Invoices", filter, accessToken, cancellationToken));
            var paymentsTask = SafeAsync(() => ListWithFilterAsync("Payments", filter, accessToken, cancellationToken));

            await Task.WhenAll(jobTask, opportunitiesTask, ticketsTask, invoicesTask, paymentsTask);

            return new JsonObject
            {
                ["jobId"] = jobId,
                ["job"] = jobTask.Result,
                ["opportunities"] = opportunitiesTask.Result,
                ["workTickets"] = ticketsTask.Result,
                ["invoices"] = invoicesTask.Result,
                ["payments"] = paymentsTask.Result,
            };
        }
    }
}
