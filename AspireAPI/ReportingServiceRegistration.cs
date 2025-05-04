using Microsoft.Extensions.DependencyInjection;
using System;
using AspireAPI.Services;

namespace AspireAPI
{
    /// <summary>
    /// Extension methods for registering reporting services
    /// </summary>
    public static class ReportingServiceRegistration
    {
        /// <summary>
        /// Adds reporting services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddReportingServices(this IServiceCollection services)
        {
            // Register all reporting-related services
            services.AddSingleton<ReportService>();
            services.AddSingleton<ReportOutputService>();
            services.AddSingleton<ReportVisualizationService>();
            services.AddSingleton<ReportTemplateService>();
            services.AddSingleton<DateRangeService>();
            
            return services;
        }
        
        /// <summary>
        /// Registers reporting-related tools with the AspireToolRouter
        /// </summary>
        /// <param name="router">The AspireToolRouter instance</param>
        /// <param name="serviceProvider">The service provider</param>
        public static void RegisterReportingTools(AspireToolRouter router, IServiceProvider serviceProvider)
        {
            // Empty method body as instructed
        }
    }
}