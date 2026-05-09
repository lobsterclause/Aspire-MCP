using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Handlers
{
    /// <summary>
    /// Hand-written tool kept for back-compat with existing MCP clients that already
    /// know the "ListContacts" name. Behaviour is identical to the generated
    /// ListContact tool — a GET against /Contacts with OData query support.
    /// </summary>
    public sealed class ListContactsHandler : GeneratedHandler
    {
        public ListContactsHandler(
            ILogger<ListContactsHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers, client) { }

        protected override string HttpMethod => "GET";
        protected override string PathTemplate => "/Contacts";
        protected override IReadOnlyList<string> QueryParameterNames { get; } =
            new[] { "$filter", "$top", "$skip", "$orderby", "$select", "$expand" };
    }
}
