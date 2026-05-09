using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Protocol.Transport;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using AspireAPI.AdminWeb;
using AspireAPI.Services;
using AspireAPI.Handlers;
using AspireAPI.ToolDefinitions;
using AspireAPI.Generated;

namespace AspireAPI;

public class Program
{
    private const int DefaultAdminPort = 5050;

    public static async Task Main(string[] args)
    {
        // Two run modes:
        //   default              -> stdio MCP server (what MCP clients launch)
        //   --admin [--port N]   -> local web admin UI for editing appsettings.Local.json
        // The admin UI binds 127.0.0.1 only and has no auth — it must never be
        // exposed on a network. The two modes are mutually exclusive so the
        // admin port can never collide with another instance launched by an
        // MCP client.
        var adminMode = args.Any(a => a == "--admin");
        if (adminMode)
        {
            await RunAdminAsync(args);
        }
        else
        {
            await RunMcpAsync(args);
        }
    }

    private static async Task RunMcpAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureSharedServices(builder.Services, builder.Configuration);
        // Wire the MCP server only in stdio mode — admin mode doesn't need it.
        builder.Services.AddSingleton<AspireMcpServer>();
        builder.Services.AddHostedService<AspireMcpServerHostedService>();
        var host = builder.Build();
        await host.RunAsync();
    }

    private static async Task RunAdminAsync(string[] args)
    {
        var port = ParsePortArg(args) ?? DefaultAdminPort;
        var builder = WebApplication.CreateBuilder(args);

        // Load Local.json so the admin UI sees current values when re-opened.
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // Bind to localhost only — admin UI is unauthenticated and surfaces
        // credentials. Refuse to listen on any other interface.
        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.Listen(IPAddress.Loopback, port);
        });

        ConfigureSharedServices(builder.Services, builder.Configuration);

        builder.Services.AddSingleton(_ => new LocalSettingsStore(builder.Environment.ContentRootPath));

        var app = builder.Build();
        app.MapAdminEndpoints();

        Console.Error.WriteLine($"[aspire-mcp] admin UI listening on http://127.0.0.1:{port}/admin");
        await app.RunAsync();
    }

    private static int? ParsePortArg(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--port" && int.TryParse(args[i + 1], out var p) && p > 0 && p <= 65535)
            {
                return p;
            }
        }
        return null;
    }

    private static void ConfigureSharedServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure options from appsettings.json (and Local override added in admin mode).
        services.Configure<AspireApiOptions>(configuration.GetSection("AspireApi"));
        services.Configure<CacheConfig>(configuration.GetSection("CacheConfig"));

        // Add essential services
        services.AddMemoryCache();
        services.AddSingleton<TokenService>();

        // Configure named HttpClient for AspireAPI
        services.AddHttpClient("AspireAPI", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AspireApiOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            client.Timeout = TimeSpan.FromSeconds(options.Timeouts.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Add general HttpClient
        services.AddHttpClient();

        // Add distributed caching if configured
        var useRedisCache = configuration.GetValue<bool>("CacheConfig:UseDistributedCaching");
        if (useRedisCache)
        {
            var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "AspireMCP:";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // Add core services
        services.AddSingleton<AdvancedCachingService>();
        services.AddSingleton<CacheManager>();
        services.AddSingleton<AdvancedFilterService>();
        services.AddSingleton<AspireApiHelpers>();
        services.AddSingleton<AspireApiService>();

        // Register all handlers
        services.AddSingleton<ListPaymentsHandler>();
        services.AddSingleton<ListPropertiesHandler>();
        services.AddSingleton<ListContactsHandler>();
        services.AddSingleton<ListJobsHandler>();

        // Register all tool definitions
        services.AddSingleton<IToolDefinition, ListPaymentsToolDefinition>();
        services.AddSingleton<IToolDefinition, ListPropertiesToolDefinition>();
        services.AddSingleton<IToolDefinition, ListContactsToolDefinition>();
        services.AddSingleton<IToolDefinition, ListJobsToolDefinition>();

        // Register the code-generated tool surface (every endpoint in the Aspire OpenAPI spec).
        services.AddGeneratedAspireTools();
    }
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
        _logger.LogInformation("Starting Aspire MCP Server (stdio transport)…");
        await _server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Aspire MCP Server…");
        await _server.StopAsync(cancellationToken);
    }
}
