using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Handlers
{
    /// <summary>
    /// Hand-written tool kept for back-compat with existing MCP clients that already
    /// know the "ListPayments" name. Behaviour is identical to the generated
    /// ListPayment tool — a GET against /Payments with OData query support.
    /// </summary>
    public sealed class ListPaymentsHandler : GeneratedHandler
    {
        public ListPaymentsHandler(
            ILogger<ListPaymentsHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers, client) { }

        protected override string HttpMethod => "GET";
        protected override string PathTemplate => "/Payments";
        protected override IReadOnlyList<string> QueryParameterNames { get; } =
            new[] { "$filter", "$top", "$skip", "$orderby", "$select", "$expand" };
    }
}
