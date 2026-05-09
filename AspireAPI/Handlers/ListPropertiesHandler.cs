using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Handlers
{
    /// <summary>
    /// Hand-written tool kept for back-compat with existing MCP clients that already
    /// know the "ListProperties" name. Behaviour is identical to the generated
    /// ListProperty tool — a GET against /Properties with OData query support.
    /// </summary>
    public sealed class ListPropertiesHandler : GeneratedHandler
    {
        public ListPropertiesHandler(
            ILogger<ListPropertiesHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers, client) { }

        protected override string HttpMethod => "GET";
        protected override string PathTemplate => "/Properties";
        protected override IReadOnlyList<string> QueryParameterNames { get; } =
            new[] { "$filter", "$top", "$skip", "$orderby", "$select", "$expand" };
    }
}
