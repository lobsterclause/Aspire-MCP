using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Protocol.Transport;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AspireAPI.Services;
using AspireAPI.Handlers;
using AspireAPI.ToolDefinitions;
using AspireAPI.Generated;

namespace AspireAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configure options from appsettings.json
        builder.Services.Configure<AspireApiOptions>(
            builder.Configuration.GetSection("AspireApi"));
        builder.Services.Configure<CacheConfig>(
            builder.Configuration.GetSection("CacheConfig"));
            
        // Add essential services
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<TokenService>();
        
        // Configure named HttpClient for AspireAPI
        builder.Services.AddHttpClient("AspireAPI", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AspireApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.Timeouts.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        // Add general HttpClient
        builder.Services.AddHttpClient();

        // Add distributed caching if configured
        var useRedisCache = builder.Configuration.GetValue<bool>("CacheConfig:UseDistributedCaching");
        if (useRedisCache)
        {
            var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "AspireMCP:";
                });
            }
            else
            {
                 // Fallback to memory cache if Redis is configured but connection string is missing
                 builder.Services.AddDistributedMemoryCache();
                 var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
                 logger?.LogWarning("Redis caching enabled but connection string is missing. Falling back to distributed memory cache.");
            }
        }
        else
        {
             // Use distributed memory cache if Redis is not configured
             builder.Services.AddDistributedMemoryCache();
        }
        
        // Add core services
        builder.Services.AddSingleton<AdvancedCachingService>();
        builder.Services.AddSingleton<CacheManager>();
        builder.Services.AddSingleton<AdvancedFilterService>();
        builder.Services.AddSingleton<AspireApiHelpers>();
        builder.Services.AddSingleton<AspireApiService>();
        
        // Register all handlers
        builder.Services.AddSingleton<ListPaymentsHandler>();
        builder.Services.AddSingleton<ListPropertiesHandler>();
        builder.Services.AddSingleton<ListContactsHandler>();
        builder.Services.AddSingleton<ListJobsHandler>();
        
        // Register all tool definitions
        builder.Services.AddSingleton<IToolDefinition, ListPaymentsToolDefinition>();
        builder.Services.AddSingleton<IToolDefinition, ListPropertiesToolDefinition>();
        builder.Services.AddSingleton<IToolDefinition, ListContactsToolDefinition>();
        builder.Services.AddSingleton<IToolDefinition, ListJobsToolDefinition>();

        // Register the code-generated tool surface (every endpoint in the Aspire OpenAPI spec).
        builder.Services.AddGeneratedAspireTools();

        // Add the MCP server
        builder.Services.AddSingleton<AspireMcpServer>();
        builder.Services.AddHostedService<AspireMcpServerHostedService>();

        // Create the host
        var host = builder.Build();
        
        // Run the host
        await host.RunAsync();
    }
    
    // Removed PrimeCacheAsync method
    // Removed WebAppHostedService class
}

public class AspireMcpServerHostedService : IHostedService
{
    private readonly AspireMcpServer _server;
    private readonly ILogger<AspireMcpServerHostedService> _logger;

    public AspireMcpServerHostedService(AspireMcpServer server, ILogger<AspireMcpServerHostedService> logger)
    {
        _server = server;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Minimal Aspire MCP Server (ListPayments only)...");
        await _server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Minimal Aspire MCP Server...");
        await _server.StopAsync(cancellationToken);
    }
}