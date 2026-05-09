using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspireAPI.Generated;

namespace AspireAPI.Compositions
{
    /// <summary>
    /// LLM-routed dispatch over the catalog. Takes a natural-language request,
    /// picks the right List* endpoint + generates an OData $filter for it,
    /// invokes the chosen endpoint, and returns the routing decision +
    /// results. One tool definition replaces the cognitive load of
    /// memorising 70 resource shapes.
    ///
    /// Uses the Anthropic Messages API directly (no SDK) so the only
    /// runtime dependency added is HTTP egress. Auth: AspireApi:AnthropicApiKey
    /// (config) or ANTHROPIC_API_KEY (env). When neither is set, the tool
    /// still appears in the catalog but returns a helpful error explaining
    /// how to configure it.
    /// </summary>
    public sealed class SearchAspireHandler : CompositionBase
    {
        // claude-haiku-4-5 is cheap, fast, and plenty smart for routing decisions
        // over a fixed catalog. Override with AspireApi:AnthropicRouterModel.
        private const string DefaultRouterModel = "claude-haiku-4-5";
        private const string AnthropicMessagesUrl = "https://api.anthropic.com/v1/messages";
        private const string AnthropicVersion = "2023-06-01";

        // Single source of truth for the routable List* surface. Keeps the system
        // prompt small (Anthropic's per-request token budget) and steers the
        // router toward Aspire's most operationally meaningful endpoints rather
        // than the long tail of lookup-table enums.
        private static readonly string[] RoutableList = {
            "Contacts", "Properties", "Jobs", "Opportunities", "WorkTickets",
            "Invoices", "Payments", "Receipts", "Vendors", "Users", "Employees",
            "Activities", "Attachments", "WorkTicketTimes", "WorkTicketVisits",
        };

        private readonly AspireApiOptions _options;
        private readonly IHttpClientFactory _httpFactory;

        public SearchAspireHandler(
            ILogger<SearchAspireHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client,
            TokenService tokenService,
            IOptions<AspireApiOptions> options)
            : base(logger, httpClientFactory, apiHelpers, client, tokenService)
        {
            _options = options.Value;
            _httpFactory = httpClientFactory;
        }

        protected override async Task<JsonNode> ComposeAsync(
            IReadOnlyDictionary<string, object?> args,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var query = Required(args, "query");
            var apiKey = ResolveAnthropicKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                return new JsonObject
                {
                    ["error"] = "SearchAspire requires an Anthropic API key. Set AspireApi:AnthropicApiKey " +
                                "in appsettings.Local.json (admin UI) or ANTHROPIC_API_KEY in the environment.",
                    ["query"] = query,
                };
            }

            var routing = await RouteAsync(query, apiKey, cancellationToken).ConfigureAwait(false);
            if (routing.Resource is null)
            {
                return new JsonObject
                {
                    ["query"] = query,
                    ["routingError"] = routing.Error ?? "router returned no resource",
                    ["routerRaw"] = routing.RawText,
                };
            }

            // Invoke the chosen list endpoint. SafeAsync wraps so a 4xx from
            // Aspire still gives the LLM the routing decision to debug from.
            var invocation = await SafeAsync(() => ListWithFilterAsync(
                routing.Resource, routing.Filter ?? "1 eq 1", accessToken, cancellationToken,
                top: routing.Top ?? 25));

            return new JsonObject
            {
                ["query"] = query,
                ["routing"] = new JsonObject
                {
                    ["resource"] = routing.Resource,
                    ["filter"] = routing.Filter,
                    ["top"] = routing.Top,
                    ["explanation"] = routing.Explanation,
                    ["model"] = routing.Model,
                },
                ["results"] = invocation,
            };
        }

        private string? ResolveAnthropicKey()
        {
            var fromOptions = _options.AnthropicApiKey;
            if (!string.IsNullOrWhiteSpace(fromOptions)) return fromOptions;
            return Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        }

        private async Task<RouterDecision> RouteAsync(
            string userQuery, string apiKey, CancellationToken cancellationToken)
        {
            var model = !string.IsNullOrWhiteSpace(_options.AnthropicRouterModel)
                ? _options.AnthropicRouterModel
                : DefaultRouterModel;

            var systemPrompt = BuildSystemPrompt();
            var requestBody = new
            {
                model,
                max_tokens = 400,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userQuery },
                },
            };

            using var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(30);
            using var msg = new HttpRequestMessage(HttpMethod.Post, AnthropicMessagesUrl)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
            };
            msg.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            msg.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);

            using var response = await http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return new RouterDecision { Error = $"Anthropic {(int)response.StatusCode}: {raw}", Model = model };
            }

            // Extract text content from Messages API response shape.
            string? text;
            try
            {
                var doc = JsonNode.Parse(raw);
                text = doc?["content"]?.AsArray().FirstOrDefault()?["text"]?.GetValue<string>();
            }
            catch (Exception ex)
            {
                return new RouterDecision { Error = $"router response unparseable: {ex.Message}", RawText = raw, Model = model };
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                return new RouterDecision { Error = "router returned empty content", RawText = raw, Model = model };
            }

            // The router is asked to emit JSON of the form
            //   {"resource":"Contacts","filter":"Type eq 'customer'","top":25,"explanation":"..."}
            // Accept either bare JSON or a fenced code block.
            var json = ExtractJson(text);
            try
            {
                var parsed = JsonNode.Parse(json);
                return new RouterDecision
                {
                    Resource = parsed?["resource"]?.GetValue<string>(),
                    Filter = parsed?["filter"]?.GetValue<string>(),
                    Top = parsed?["top"]?.GetValue<int?>(),
                    Explanation = parsed?["explanation"]?.GetValue<string>(),
                    RawText = text,
                    Model = model,
                };
            }
            catch (Exception ex)
            {
                return new RouterDecision { Error = $"router JSON parse failed: {ex.Message}", RawText = text, Model = model };
            }
        }

        private static string BuildSystemPrompt()
        {
            var resources = string.Join(", ", RoutableList);
            return $$"""
You are a routing function for the Aspire field-service API. Given a user's
natural-language query, decide which Aspire collection to query and what OData
$filter to apply. Reply with ONLY a JSON object — no prose, no code fences.

Available resources (singular forms also valid):
{{resources}}

OData filter syntax cheatsheet:
- Equality: ContactID eq 123, Type eq 'customer'
- Range:   LastModifiedDateTime ge 2026-01-01, Amount gt 1000
- Combine: A and B, A or B
- String:  contains(Name, 'Smith')
- Quote string literals with single quotes; numbers/IDs unquoted; ISO datetimes unquoted.

Reply shape:
{
  "resource":   "<one of the available resources, exact case>",
  "filter":     "<OData expression, or empty string for unfiltered>",
  "top":        <integer 1-100 — how many records to return>,
  "explanation":"<one short sentence explaining the choice>"
}

If the query is ambiguous, pick the most likely resource and use a permissive filter.
""";
        }

        private static string ExtractJson(string text)
        {
            text = text.Trim();
            // Strip ```json ... ``` fences if present.
            if (text.StartsWith("```"))
            {
                var firstNewline = text.IndexOf('\n');
                if (firstNewline > 0)
                {
                    text = text[(firstNewline + 1)..];
                    var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
                    if (lastFence > 0) text = text[..lastFence];
                }
            }
            return text.Trim();
        }

        private sealed class RouterDecision
        {
            public string? Resource;
            public string? Filter;
            public int? Top;
            public string? Explanation;
            public string? Error;
            public string? RawText;
            public string? Model;
        }
    }

    public sealed class SearchAspireToolDefinition : CompositionToolDefinitionBase
    {
        public override string Name => "SearchAspire";
        public override string Description =>
            "[composition] Natural-language search across Aspire. A small LLM router (Claude " +
            "Haiku) picks the right List* endpoint and synthesizes the OData $filter from your " +
            "phrase. Requires AspireApi:AnthropicApiKey or the ANTHROPIC_API_KEY env var.";
        protected override string SchemaJson => """
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "query": {
      "type": "string",
      "description": "Natural-language query, e.g. \"open invoices for Smith Properties last quarter\"."
    }
  },
  "required": ["query"]
}
""";
    }
}
