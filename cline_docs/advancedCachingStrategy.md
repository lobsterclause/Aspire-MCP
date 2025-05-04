# Advanced Caching Strategy for Aspire MCP Server

This document outlines the advanced caching strategy implemented in the Aspire MCP Server to improve performance, reliability, and scalability.

## Overview

The advanced caching system provides a sophisticated, multi-tiered caching architecture with intelligent invalidation, adaptive Time-To-Live (TTL), monitoring capabilities, and support for distributed caching. It is designed to significantly enhance performance while maintaining data consistency.

## Key Features

- **Multi-level caching**: In-memory caching with optional distributed caching (Redis)
- **Intelligent cache invalidation**: Automatically invalidates related entities when data changes
- **Adaptive TTL**: Dynamically adjusts cache durations based on access patterns
- **Cache monitoring**: Comprehensive statistics for monitoring and optimization
- **Cache priming**: Pre-loads commonly accessed data for faster initial responses
- **Performance metrics**: Tracks cache hit/miss ratios and response times
- **Memory management**: Automatically evicts least-recently-used items to prevent memory issues
- **REST API endpoints**: For cache management and monitoring

## Architecture

The advanced caching system consists of the following components:

1. **AdvancedCachingService**: Core service that handles cache operations, monitoring, and management
2. **CacheManager**: High-level service that coordinates caching operations and smart invalidation
3. **CacheController**: REST API for cache management and monitoring
4. **Legacy CachingService**: Backward-compatible service that delegates to the new system

### Caching Layers

The system supports two caching layers:

1. **Memory Cache**: Fast, in-process cache for frequently accessed data
2. **Distributed Cache**: Optional Redis-based cache for sharing between instances

## Configuration

Configure the caching system in `appsettings.json`:

```json
"CacheConfig": {
  "DefaultCacheDurations": {
    "timeentries": "00:05:00",
    "contacts": "00:15:00",
    "divisions": "00:30:00",
    "branches": "00:30:00",
    "inventoryitems": "00:15:00",
    "jobs": "00:10:00",
    "worktickets": "00:10:00",
    "invoices": "00:15:00",
    "opportunities": "00:15:00",
    "properties": "00:20:00",
    "equipment": "00:25:00",
    "default": "00:10:00"
  },
  "EnableMonitoring": true,
  "EnableAdaptiveTtl": true,
  "UseDistributedCaching": false,
  "MemoryCacheSizeLimit": 1000,
  "CleanupIntervalSeconds": 300
},
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

### Configuration Options

- **DefaultCacheDurations**: Cache durations for each entity type
- **EnableMonitoring**: Enables cache statistics collection
- **EnableAdaptiveTtl**: Dynamically adjusts cache durations based on access patterns
- **UseDistributedCaching**: Enables Redis distributed caching
- **MemoryCacheSizeLimit**: Maximum number of items in memory cache
- **CleanupIntervalSeconds**: How often to run cleanup operations
- **Redis:ConnectionString**: Connection string for Redis (when distributed caching is enabled)

## Usage

### Basic Usage in Services

Services should use the `CacheManager` to interact with the cache:

```csharp
public class MyService
{
    private readonly CacheManager _cacheManager;
    
    public MyService(CacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }
    
    public async Task<MyData> GetDataAsync(string id)
    {
        return await _cacheManager.GetFromCacheOrFetchAsync<MyData>(
            "myentity",
            new Dictionary<string, object> { ["id"] = id },
            async () => {
                // This is only executed on cache miss
                return await FetchDataFromSourceAsync(id);
            });
    }
    
    public async Task UpdateDataAsync(string id, MyData data)
    {
        // Update data in the database
        await UpdateInDatabaseAsync(id, data);
        
        // Invalidate cache for this entity and related entities
        await _cacheManager.InvalidateEntityAndRelatedCacheAsync("myentity");
    }
}
```

### Cache Invalidation

When data changes, invalidate the cache for the affected entity and its related entities:

```csharp
await _cacheManager.InvalidateEntityAndRelatedCacheAsync("properties");
```

The system will automatically invalidate related entities based on the predefined relationships in `CacheManager`.

## Cache Management API

The system exposes the following REST API endpoints for cache management:

- **GET /api/cache/stats**: Get cache statistics
- **POST /api/cache/invalidate/{entityType}**: Invalidate cache for an entity type
- **POST /api/cache/invalidate-related**: Invalidate cache for an entity and related entities
- **DELETE /api/cache/key/{cacheKey}**: Remove a specific cache key
- **POST /api/cache/prime**: Prime the cache with common data

## Entity Relationships for Cache Invalidation

The cache system automatically invalidates related entities based on the following relationships:

```csharp
private static readonly Dictionary<string, HashSet<string>> _entityRelationships = new()
{
    ["properties"] = new HashSet<string> { "contacts", "locations" },
    ["contacts"] = new HashSet<string> { "properties", "jobs", "opportunities" },
    ["jobs"] = new HashSet<string> { "contacts", "divisions", "worktickets" },
    ["worktickets"] = new HashSet<string> { "jobs", "equipment" },
    ["invoices"] = new HashSet<string> { "jobs", "contacts" },
    ["equipment"] = new HashSet<string> { "worktickets", "inventory" },
    ["opportunities"] = new HashSet<string> { "contacts", "divisions" },
    ["divisions"] = new HashSet<string> { "jobs", "opportunities" },
    ["branches"] = new HashSet<string> { "divisions", "jobs" },
    ["inventoryitems"] = new HashSet<string> { "equipment" }
};
```

## Cache Monitoring and Statistics

The system collects the following statistics for each entity type:

- **Total Requests**: Total number of cache lookups
- **Hits**: Number of cache hits
- **Misses**: Number of cache misses
- **Hit Ratio**: Percentage of cache hits
- **Average Response Time**: Average time to serve a request
- **Average Hit Time**: Average time for a cache hit
- **Average Miss Time**: Average time for a cache miss (including data fetch)

Access these statistics via the REST API endpoint: `GET /api/cache/stats`

## Best Practices

1. **Use CacheManager**: Always use `CacheManager` instead of directly using `AdvancedCachingService`
2. **Invalidate Related Entities**: When data changes, invalidate related entities
3. **Monitor Cache Performance**: Regularly check cache statistics to identify opportunities for optimization
4. **Adjust Cache Durations**: Set appropriate cache durations based on data volatility
5. **Prime the Cache**: Pre-load commonly accessed data at application startup
6. **Consider Entity Size**: Cache smaller entities for longer periods
7. **Enable Distributed Caching**: For multi-instance deployments, enable Redis distributed caching

## Troubleshooting

### Common Issues

**Issue**: High cache miss ratio
- **Solution**: Verify cache durations, adjust if needed, or prime the cache with common data

**Issue**: Memory usage too high
- **Solution**: Reduce `MemoryCacheSizeLimit` or reduce cache durations

**Issue**: Stale data after updates
- **Solution**: Ensure proper cache invalidation is happening after data modifications

### Logging

The caching system logs detailed information about its operations. Configure the logging level in `appsettings.json`:

```json
"Logging": {
  "LogLevel": {
    "AspireAPI.Services.AdvancedCachingService": "Debug"
  }
}
```

## Future Enhancements

Planned future enhancements include:

1. **Cache compression**: Compress large cache items to reduce memory usage
2. **Background refresh**: Asynchronously refresh cache items before expiration
3. **Circuit breaker pattern**: Fallback to cached data when backend services are unavailable
4. **Cache warming**: Proactively warm the cache based on predicted access patterns
5. **Cache segmentation**: Segment cache by user or tenant for multi-tenant scenarios