using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using AspireAPI.Handlers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace AspireAPI
{
    /// <summary>
    /// Routes tool requests to the appropriate handlers
    /// </summary>
    public class AspireToolRouter
    {
        private readonly Dictionary<string, Func<IServiceProvider, BaseHandler>> _toolHandlers;
        private readonly IServiceProvider _serviceProvider;

        public AspireToolRouter(IServiceProvider serviceProvider)
        {
            _toolHandlers = new Dictionary<string, Func<IServiceProvider, BaseHandler>>(StringComparer.OrdinalIgnoreCase);
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registers a tool handler with the router
        /// </summary>
        /// <param name="toolName">The name of the tool</param>
        /// <param name="handlerFactory">A factory function that returns the handler for the tool</param>
        public void RegisterTool(string toolName, Func<IServiceProvider, BaseHandler> handlerFactory)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                throw new ArgumentNullException(nameof(toolName));
            }

            if (handlerFactory == null)
            {
                throw new ArgumentNullException(nameof(handlerFactory));
            }

            _toolHandlers[toolName] = handlerFactory;
        }

        /// <summary>
        /// Gets the handler for a tool
        /// </summary>
        /// <param name="toolName">The name of the tool</param>
        /// <param name="serviceProvider">The service provider to use to create the handler</param>
        /// <returns>The handler for the tool, or null if the tool is not registered</returns>
        public BaseHandler GetToolHandler(string toolName, IServiceProvider serviceProvider)
        {
            if (_toolHandlers.TryGetValue(toolName, out var handlerFactory))
            {
                return handlerFactory(serviceProvider);
            }

            return null;
        }

        /// <summary>
        /// Checks if a tool is registered
        /// </summary>
        /// <param name="toolName">The name of the tool</param>
        /// <returns>True if the tool is registered, false otherwise</returns>
        public bool HasTool(string toolName)
        {
            return _toolHandlers.ContainsKey(toolName);
        }

        /// <summary>
        /// Gets the names of all registered tools
        /// </summary>
        /// <returns>A list of all registered tool names</returns>
        public IEnumerable<string> GetToolNames()
        {
            return _toolHandlers.Keys;
        }

        /// <summary>
        /// Registers all reporting tools with the router
        /// This is a wrapper around the extension method in ReportingServiceRegistration
        /// </summary>
        public void RegisterReportingTools()
        {
            // This will call the extension method defined in ReportingServiceRegistration.cs
            ReportingServiceRegistration.RegisterReportingTools(this, _serviceProvider);
        }
    }
}