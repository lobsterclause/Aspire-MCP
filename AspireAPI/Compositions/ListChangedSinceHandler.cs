using System;
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
    /// Aspire has no webhooks, so the cheapest "what changed since X" primitive is a
    /// fan-out of $filter=LastModifiedDateTime ge {since} across the entity types
    /// the operator cares about. Returns a {entity → records} map.
    ///
    /// Defaults to a useful slice (Contacts, Properties, Opportunities, Jobs,
    /// WorkTickets, Invoices, Payments, Receipts) — pass `entities: ["..."]` to
    /// override.
    /// </summary>
    public sealed class ListChangedSinceHandler : CompositionBase
    {
        // Entities that expose LastModifiedDateTime per the Aspire OpenAPI spec
        // (verified Nov 2025). Stored alphabetically for deterministic output.
        private static readonly string[] DefaultEntities =
        {
            "Contacts", "Invoices", "Jobs", "Opportunities", "Payments",
            "Properties", "Receipts", "WorkTickets", "WorkTicketTimes",
        };

        public ListChangedSinceHandler(
            ILogger<ListChangedSinceHandler> logger,
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
            var since = Required(args, "since"); // ISO 8601 timestamp
            var entities = ResolveEntities(args);

            var filter = $"LastModifiedDateTime ge {since}";
            var fetches = entities
                .Select(e => (entity: e, task: SafeAsync(() =>
                    ListWithFilterAsync(e, filter, accessToken, cancellationToken, top: 500))))
                .ToArray();

            await Task.WhenAll(fetches.Select(f => f.task));

            var result = new JsonObject
            {
                ["since"] = since,
                ["entities"] = new JsonArray(entities.Select(e => (JsonNode)e).ToArray()),
            };
            var data = new JsonObject();
            foreach (var (entity, task) in fetches)
            {
                data[entity] = task.Result;
            }
            result["data"] = data;
            return result;
        }

        private static string[] ResolveEntities(IReadOnlyDictionary<string, object?> args)
        {
            if (!args.TryGetValue("entities", out var v) || v is null) return DefaultEntities;
            return v switch
            {
                JsonElement je when je.ValueKind == JsonValueKind.Array
                    => je.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s.Length > 0).ToArray(),
                IEnumerable<object> enumerable
                    => enumerable.Select(x => x?.ToString() ?? "").Where(s => s.Length > 0).ToArray(),
                _ => DefaultEntities,
            };
        }
    }
}
