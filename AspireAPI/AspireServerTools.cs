using ModelContextProtocol.Protocol.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Handlers;

namespace AspireAPI;

/// <summary>
/// Partial class containing tool handler implementations for AspireMcpServer
/// </summary>
public partial class AspireMcpServer
{
    // These handlers are already defined in AspireMcpServer.cs
    // No need to redefine them here since this is a partial class
    
    // Router population now happens at DI registration time inside
    // AspireToolRouter's singleton factory (see Program.ConfigureSharedServices).
    // This partial used to register tools here; left as a no-op for back-compat
    // with any external caller still invoking it.
    private void RegisterToolHandlers() { }

    /// <summary>
    /// Handle ListPayments tool requests with advanced OData query support
    /// </summary>
    internal async Task<CallToolResponse> HandleListPaymentsAsync(
        IDictionary<string, object> arguments,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await _listPaymentsHandler.HandleAsync(arguments, accessToken, cancellationToken);
    }
    
    /// <summary>
    /// Handle ListProperties tool requests
    /// </summary>
    internal async Task<CallToolResponse> HandleListPropertiesAsync(
        IDictionary<string, object> arguments,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await _listPropertiesHandler.HandleAsync(arguments, accessToken, cancellationToken);
    }
    
    /// <summary>
    /// Handle ListContacts tool requests
    /// </summary>
    internal async Task<CallToolResponse> HandleListContactsAsync(
        IDictionary<string, object> arguments,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await _listContactsHandler.HandleAsync(arguments, accessToken, cancellationToken);
    }
    
    /// <summary>
    /// Handle ListJobs tool requests
    /// </summary>
    internal async Task<CallToolResponse> HandleListJobsAsync(
        IDictionary<string, object> arguments,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await _listJobsHandler.HandleAsync(arguments, accessToken, cancellationToken);
    }
}