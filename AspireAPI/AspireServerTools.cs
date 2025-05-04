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
    
    /// <summary>
    /// Register tool handlers with the router
    /// </summary>
    private void RegisterToolHandlers()
    {
        // Register the activated tools
        _toolRouter.RegisterTool("ListPayments", provider => _listPaymentsHandler);
        _toolRouter.RegisterTool("ListProperties", provider => _listPropertiesHandler);
        _toolRouter.RegisterTool("ListContacts", provider => _listContactsHandler);
        _toolRouter.RegisterTool("ListJobs", provider => _listJobsHandler);
        
        // Other tools can be registered as needed
        // _toolRouter.RegisterToolHandler("GetTimeEntryReport", HandleGetTimeEntryReportAsync);
        // _toolRouter.RegisterToolHandler("ListDivisions", HandleListDivisionsAsync);
        // _toolRouter.RegisterToolHandler("ListOpportunities", HandleListOpportunitiesAsync);
        // ...
    }

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