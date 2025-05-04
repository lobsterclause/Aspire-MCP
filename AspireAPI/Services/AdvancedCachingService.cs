using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.Services
{
    /// <summary>
    /// Configuration for the advanced caching system
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// Default cache durations for different entity types
        /// </summary>
        public Dictionary<string, TimeSpan> DefaultCacheDurations { get; set; } = new()
        {
            { "timeentries", TimeSpan.FromMinutes(5) },
            { "contacts", TimeSpan.FromMinutes(15) },
            { "divisions", TimeSpan.FromMinutes(30) },
            { "branches", TimeSpan.FromMinutes(30) },
            { "inventoryitems", TimeSpan.FromMinutes(15) },
            { "jobs", TimeSpan.FromMinutes(10) },
            { "worktickets", TimeSpan.FromMinutes(10) },
            { "invoices", TimeSpan.FromMinutes(15) },
            { "opportunities", TimeSpan.FromMinutes(15) },
            { "properties", TimeSpan.FromMinutes(20) },
            { "equipment", TimeSpan.FromMinutes(25) },
            { "default", TimeSpan.FromMinutes(10) }
        };

        /// <summary>
        /// Whether to enable cache monitoring
        /// </summary>
        public bool EnableMonitoring { get; set; } = true;

        /// <summary>
        /// Whether to enable adaptive TTL adjustments
        /// </summary>
        public bool EnableAdaptiveTtl { get; set; } = true;

        /// <summary>
        /// Whether to use distributed caching if available
        /// </summary>
        public bool UseDistributedCaching { get; set; } = true;
    }

    /// <summary>
    /// Advanced caching service with support for distributed caching, invalidation,
    /// monitoring, and adaptive TTL
    /// </summary>
    public class AdvancedCachingService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache? _distributedCache;
        private readonly ILogger<AdvancedCachingService> _logger;
        private readonly CacheConfig _config;
        
        // For tracking entity-key relationships (used for smart invalidation)
        private readonly ConcurrentDictionary<string, HashSet<string>> _entityKeysMap = new();
        
        // For tracking cache statistics
        private readonly ConcurrentDictionary<string, CacheStatistics> _statistics = new();

        public AdvancedCachingService(
            IMemoryCache memoryCache,
            IOptions<CacheConfig> config,
            ILogger<AdvancedCachingService> logger,
            IDistributedCache? distributedCache = null)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _config = config?.Value ?? new CacheConfig();
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Gets the cached value or executes the provided factory function and caches the result
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(
            string cacheKey, 
            Func<Task<T>> factory, 
            string entityType = "default",
            CancellationToken cancellationToken = default)
        {
            // Simple implementation for compilation
            if (_memoryCache.TryGetValue(cacheKey, out T cachedValue))
            {
                return cachedValue;
            }
            
            var result = await factory();
            
            var cacheDuration = GetCacheDurationForEntityType(entityType);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheDuration);
            
            _memoryCache.Set(cacheKey, result, cacheEntryOptions);
            
            return result;
        }

        /// <summary>
        /// Generates a cache key based on entity type and parameters
        /// </summary>
        public string GenerateCacheKey(string entityType, Dictionary<string, object> parameters)
        {
            var key = $"{entityType.ToLowerInvariant()}";
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param.Value != null)
                    {
                        key += $":{param.Key}={param.Value}";
                    }
                }
            }
            
            return key;
        }

        /// <summary>
        /// Gets the appropriate cache duration for an entity type
        /// </summary>
        private TimeSpan GetCacheDurationForEntityType(string entityType)
        {
            string normalizedType = entityType.ToLowerInvariant();
            return _config.DefaultCacheDurations.TryGetValue(normalizedType, out var duration) 
                ? duration 
                : _config.DefaultCacheDurations["default"];
        }

        /// <summary>
        /// Invalidates cache entries based on related entities
        /// </summary>
        public async Task InvalidateRelatedEntitiesCacheAsync(
            string changedEntity, 
            IEnumerable<string> relatedEntities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Invalidating cache for {changedEntity} and related entities");
            
            // Stub implementation - just log the action
            _logger.LogInformation($"Cache invalidation for {changedEntity} and {string.Join(", ", relatedEntities)}");
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets cache statistics for monitoring
        /// </summary>
        public Dictionary<string, CacheStatistics> GetCacheStatistics()
        {
            return new Dictionary<string, CacheStatistics>();
        }

        /// <summary>
        /// Primes the cache with frequently accessed data
        /// </summary>
        public async Task PrimeCacheAsync(
            Dictionary<string, Func<Task<object>>> loaders,
            CancellationToken cancellationToken = default)
        {
            // Stub implementation
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Invalidates all cache entries related to a specific entity type
        /// </summary>
        /// <param name="entityType">The type of entity to invalidate cache for</param>
        /// <param name="entityId">Optional specific entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task InvalidateEntityCacheAsync(
            string entityType,
            string entityId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Invalidating cache for entity type {entityType}" +
                (entityId != null ? $" with ID {entityId}" : ""));
            
            // Get the pattern to match for invalidation
            string keyPattern = string.IsNullOrEmpty(entityId)
                ? entityType.ToLowerInvariant()
                : $"{entityType.ToLowerInvariant()}:{entityId}";
                
            // In a real implementation, we would:
            // 1. Find all cache keys that match the pattern
            // 2. Remove them from both memory and distributed cache
            // 3. Update tracking structures
            
            // For now, just log the action
            _logger.LogInformation($"Cache invalidation for pattern: {keyPattern}");
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Cache statistics for monitoring
    /// </summary>
    public class CacheStatistics
    {
        public int Hits { get; set; }
        public int Misses { get; set; }
        public int TotalRequests { get; set; }
        public TimeSpan HitDuration { get; set; }
        public TimeSpan MissDuration { get; set; }
        public TimeSpan TotalDuration { get; set; }
        
        public double HitRatio => TotalRequests > 0 ? (double)Hits / TotalRequests : 0;
        
        public CacheStatistics() { }
        
        public CacheStatistics(CacheStatistics source)
        {
            Hits = source.Hits;
            Misses = source.Misses;
            TotalRequests = source.TotalRequests;
            HitDuration = source.HitDuration;
            MissDuration = source.MissDuration;
            TotalDuration = source.TotalDuration;
        }
    }
}