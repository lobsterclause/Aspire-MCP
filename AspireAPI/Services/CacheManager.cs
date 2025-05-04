using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.Services
{
    /// <summary>
    /// Cache manager that provides common caching patterns and functions
    /// to be used by other services
    /// </summary>
    public class CacheManager
    {
        private readonly AdvancedCachingService _cachingService;
        private readonly ILogger<CacheManager> _logger;
        
        // Entity relationship map for smart cache invalidation
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

        public CacheManager(
            AdvancedCachingService cachingService,
            ILogger<CacheManager> logger)
        {
            _cachingService = cachingService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves data from cache or executes the provided factory to fetch
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="entityType">The entity type</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="factory">Factory function to fetch data if not cached</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The data, from cache if available or freshly fetched</returns>
        public async Task<T> GetFromCacheOrFetchAsync<T>(
            string entityType,
            Dictionary<string, object> parameters,
            Func<Task<T>> factory,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = _cachingService.GenerateCacheKey(entityType, parameters);
            return await _cachingService.GetOrCreateAsync(cacheKey, factory, entityType, cancellationToken);
        }

        /// <summary>
        /// Invalidates cache for an entity when it changes, and also invalidates related entities
        /// </summary>
        /// <param name="entityType">The entity type that was modified</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task InvalidateEntityAndRelatedCacheAsync(
            string entityType,
            CancellationToken cancellationToken = default)
        {
            var normalizedType = entityType.ToLowerInvariant();
            var relatedEntities = GetRelatedEntities(normalizedType);
            
            await _cachingService.InvalidateRelatedEntitiesCacheAsync(
                normalizedType,
                relatedEntities,
                cancellationToken);
        }

        /// <summary>
        /// Gets entities related to the specified entity type based on predefined relationships
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <returns>Set of related entity types</returns>
        public HashSet<string> GetRelatedEntities(string entityType)
        {
            return _entityRelationships.TryGetValue(entityType.ToLowerInvariant(), out var related)
                ? related
                : new HashSet<string>();
        }

        /// <summary>
        /// Primes the cache with frequently accessed data on application startup
        /// </summary>
        public async Task PrimeCommonCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Priming cache with frequently accessed data");
                
                // Create a dictionary of data loaders for common entities
                var loaders = new Dictionary<string, Func<Task<object>>>
                {
                    // Simplified implementation
                    ["divisions"] = async () => {
                        await Task.Delay(10, cancellationToken);
                        return new { divisions = new object[] { } };
                    },
                    ["branches"] = async () => {
                        await Task.Delay(10, cancellationToken);
                        return new { branches = new object[] { } };
                    }
                };
                
                await _cachingService.PrimeCacheAsync(loaders, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error priming common cache");
                // Continue without priming - this shouldn't prevent application startup
            }
        }

        /// <summary>
        /// Gets cache statistics for monitoring and analysis
        /// </summary>
        public Dictionary<string, CacheStatistics> GetCacheStatistics()
        {
            return _cachingService.GetCacheStatistics();
        }
    }
}