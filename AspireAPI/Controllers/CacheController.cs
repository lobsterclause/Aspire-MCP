using AspireAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.Controllers
{
    [ApiController]
    [Route("api/cache")]
    public class CacheController : ControllerBase
    {
        private readonly AdvancedCachingService _cachingService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(
            AdvancedCachingService cachingService,
            ILogger<CacheController> logger)
        {
            _cachingService = cachingService;
            _logger = logger;
        }

        /// <summary>
        /// Gets cache statistics for monitoring
        /// </summary>
        [HttpGet("stats")]
        public ActionResult GetCacheStatistics()
        {
            try
            {
                var stats = _cachingService.GetCacheStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache statistics");
                return StatusCode(500, "Error retrieving cache statistics");
            }
        }

        /// <summary>
        /// Invalidates all cache entries for a specific entity type
        /// </summary>
        [HttpPost("invalidate/{entityType}")]
        public async Task<ActionResult> InvalidateEntityCache(string entityType, CancellationToken cancellationToken)
        {
            try
            {
                await _cachingService.InvalidateEntityCacheAsync(entityType, cancellationToken);
                return Ok(new { message = $"Cache for entity type '{entityType}' has been invalidated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error invalidating cache for entity type {entityType}");
                return StatusCode(500, $"Error invalidating cache for entity type {entityType}");
            }
        }

        /// <summary>
        /// Invalidates cache entries for an entity and its related entities
        /// </summary>
        [HttpPost("invalidate-related")]
        public async Task<ActionResult> InvalidateRelatedEntitiesCache(
            [FromBody] InvalidateRelatedRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.EntityType))
                {
                    return BadRequest("Entity type is required");
                }

                await _cachingService.InvalidateRelatedEntitiesCacheAsync(
                    request.EntityType,
                    request.RelatedEntities ?? new List<string>(),
                    cancellationToken);

                return Ok(new { message = $"Cache for '{request.EntityType}' and related entities has been invalidated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating related entity caches");
                return StatusCode(500, "Error invalidating related entity caches");
            }
        }

        /// <summary>
        /// Manually removes a specific cache key
        /// </summary>
        [HttpDelete("key/{cacheKey}")]
        public async Task<ActionResult> RemoveCacheKey(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _cachingService.RemoveFromCacheAsync(cacheKey, cancellationToken);
                return Ok(new { message = $"Cache key '{cacheKey}' has been removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache key {cacheKey}");
                return StatusCode(500, $"Error removing cache key {cacheKey}");
            }
        }

        /// <summary>
        /// Manually primes the cache with common data
        /// </summary>
        [HttpPost("prime")]
        public async Task<ActionResult> PrimeCache(CancellationToken cancellationToken)
        {
            try
            {
                // This is a simplified example - in a real implementation,
                // you would define the appropriate loaders for common data
                var loaders = new Dictionary<string, Func<Task<object>>>
                {
                    ["divisions"] = async () => {
                        // Example placeholder - this should be implemented with actual data fetching
                        await Task.Delay(100, cancellationToken);
                        return new object();
                    },
                    ["branches"] = async () => {
                        // Example placeholder - this should be implemented with actual data fetching
                        await Task.Delay(100, cancellationToken);
                        return new object();
                    }
                };

                await _cachingService.PrimeCacheAsync(loaders, cancellationToken);
                return Ok(new { message = "Cache has been primed with common data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error priming cache");
                return StatusCode(500, "Error priming cache");
            }
        }
    }

    public class InvalidateRelatedRequest
    {
        public string EntityType { get; set; }
        public List<string> RelatedEntities { get; set; }
    }
}