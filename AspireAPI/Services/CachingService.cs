using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspireAPI.Services;

namespace AspireAPI
{
    /// <summary>
    /// Legacy service responsible for caching data from Aspire API to improve performance
    /// This now acts as a facade to the new AdvancedCachingService for backward compatibility
    /// </summary>
    public class CachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingService> _logger;
        private readonly AdvancedCachingService _advancedCachingService;
        private readonly CacheManager _cacheManager;

        // Default cache durations for different entity types
        private static readonly Dictionary<string, TimeSpan> _defaultCacheDurations = new()
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
            { "default", TimeSpan.FromMinutes(10) }
        };

        public CachingService(
            IMemoryCache cache,
            ILogger<CachingService> logger,
            AdvancedCachingService advancedCachingService = null,
            CacheManager cacheManager = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _advancedCachingService = advancedCachingService;
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Gets the cached value or executes the provided factory function and caches the result
        /// </summary>
        /// <typeparam name="T">The type of the cached item</typeparam>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="factory">The factory function to execute if item not in cache</param>
        /// <param name="entityType">The entity type for determining cache duration</param>
        /// <returns>The cached or newly retrieved value</returns>
        public async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, string entityType = "default")
        {
            // Use advanced caching if available
            if (_advancedCachingService != null)
            {
                return await _advancedCachingService.GetOrCreateAsync(cacheKey, factory, entityType);
            }
            
            // Fall back to original implementation
            if (_cache.TryGetValue(cacheKey, out T cachedValue))
            {
                _logger.LogInformation($"Cache hit for key: {cacheKey}");
                return cachedValue;
            }

            _logger.LogInformation($"Cache miss for key: {cacheKey}, retrieving data");
            var result = await factory();

            if (result != null)
            {
                var cacheDuration = GetCacheDurationForEntityType(entityType);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(cacheDuration);
                
                _cache.Set(cacheKey, result, cacheEntryOptions);
                _logger.LogInformation($"Cached data for key: {cacheKey} with duration: {cacheDuration}");
            }

            return result;
        }

        /// <summary>
        /// Generates a cache key based on entity type and parameters
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="parameters">The parameters used for the API call</param>
        /// <returns>A unique cache key</returns>
        public string GenerateCacheKey(string entityType, Dictionary<string, object> parameters)
        {
            // Create a unique key based on entity type and sorted parameters
            var key = $"{entityType.ToLowerInvariant()}";
            
            if (parameters != null)
            {
                var sortedParams = parameters.OrderBy(p => p.Key);
                foreach (var param in sortedParams)
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
        /// <param name="entityType">The entity type</param>
        /// <returns>The cache duration</returns>
        private TimeSpan GetCacheDurationForEntityType(string entityType)
        {
            string normalizedType = entityType.ToLowerInvariant();
            return _defaultCacheDurations.TryGetValue(normalizedType, out var duration) 
                ? duration 
                : _defaultCacheDurations["default"];
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="cacheKey">The cache key to remove</param>
        public void RemoveFromCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
            _logger.LogInformation($"Removed from cache: {cacheKey}");
        }

        /// <summary>
        /// Invalidates all cache entries related to a specific entity type
        /// </summary>
        /// <param name="entityType">The entity type</param>
        public async Task InvalidateEntityCache(string entityType)
        {
            // Use advanced caching if available
            if (_advancedCachingService != null)
            {
                await _advancedCachingService.InvalidateEntityCacheAsync(entityType);
                return;
            }
            
            // Legacy implementation - log only
            _logger.LogInformation($"Cache invalidation for entity type {entityType} would be performed here");
        }
        
        /// <summary>
        /// Invalidates cache entries with a specific prefix
        /// </summary>
        /// <param name="prefix">The prefix to match</param>
        public async Task InvalidateByPrefix(string prefix)
        {
            // Use cache manager if available for smart invalidation
            if (_cacheManager != null)
            {
                await _cacheManager.InvalidateEntityAndRelatedCacheAsync(prefix);
                return;
            }
            
            // Use advanced caching if available
            if (_advancedCachingService != null)
            {
                await _advancedCachingService.InvalidateEntityCacheAsync(prefix);
                return;
            }
            
            // Legacy implementation - log only
            _logger.LogInformation($"Cache invalidation for prefix {prefix} would be performed here");
        }
    }
}