using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Handlers
{
    /// <summary>
    /// Hand-written tool kept for back-compat with existing MCP clients that already
    /// know the "ListJobs" name. Behaviour is identical to the generated ListJob tool
    /// — a GET against /Jobs with OData query support.
    /// </summary>
    public sealed class ListJobsHandler : GeneratedHandler
    {
        public ListJobsHandler(
            ILogger<ListJobsHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers, client) { }

        protected override string HttpMethod => "GET";
        protected override string PathTemplate => "/Jobs";
        protected override IReadOnlyList<string> QueryParameterNames { get; } =
            new[] { "$filter", "$top", "$skip", "$orderby", "$select", "$expand" };
    }
}
