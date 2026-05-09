using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Compositions
{
    /// <summary>
    /// Returns a calendar-grid-shaped payload of work-ticket visits for a given date,
    /// optionally filtered to a branch. Bundles routes + crews + visits so the LLM
    /// can render a schedule without three follow-up calls.
    /// </summary>
    public sealed class RenderScheduleBoardHandler : CompositionBase
    {
        public RenderScheduleBoardHandler(
            ILogger<RenderScheduleBoardHandler> logger,
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
            var date = Required(args, "date");          // ISO yyyy-MM-dd
            var branchId = GetString(args, "branchId"); // optional

            // Build the visit filter — date is the required pivot, branch optional.
            var visitFilter = $"ScheduleDate eq {date}";
            if (!string.IsNullOrEmpty(branchId))
            {
                visitFilter += $" and BranchID eq {branchId}";
            }

            var visitsTask = SafeAsync(() => ListWithFilterAsync("WorkTicketVisits", visitFilter, accessToken, cancellationToken, top: 500));
            var routesTask = SafeAsync(() => ListWithFilterAsync("Routes",
                string.IsNullOrEmpty(branchId) ? "1 eq 1" : $"BranchID eq {branchId}",
                accessToken, cancellationToken));

            await Task.WhenAll(visitsTask, routesTask);

            // Group visits by RouteID so the UI can render route lanes directly.
            var visits = visitsTask.Result;
            var grouped = new JsonObject();
            if (visits is JsonObject vobj && vobj["data"] is JsonArray varr)
            {
                foreach (var v in varr)
                {
                    if (v is not JsonObject vo) continue;
                    var routeId = vo["RouteID"]?.ToString() ?? "_unrouted";
                    if (grouped[routeId] is not JsonArray lane)
                    {
                        lane = new JsonArray();
                        grouped[routeId] = lane;
                    }
                    lane.Add(JsonNode.Parse(vo.ToJsonString())!);
                }
            }

            return new JsonObject
            {
                ["date"] = date,
                ["branchId"] = branchId,
                ["routes"] = routesTask.Result,
                ["visitsByRoute"] = grouped,
                ["rawVisits"] = visits,
            };
        }
    }
}
