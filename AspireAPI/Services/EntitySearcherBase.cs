using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Models;

namespace AspireAPI.Services
{
    /// <summary>
    /// Base class for all entity searchers
    /// </summary>
    public abstract class EntitySearcherBase
    {
        protected readonly AspireApiService AspireApi;
        protected readonly ILogger _logger;

        protected EntitySearcherBase(AspireApiService aspireApi, ILogger logger)
        {
            AspireApi = aspireApi ?? throw new ArgumentNullException(nameof(aspireApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Search for entities matching the given criteria
        /// </summary>
        public abstract Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken);

        /// <summary>
        /// Calculate a match score between a field value and a search term
        /// </summary>
        protected int CalculateMatchScore(string fieldValue, string searchTerm)
        {
            if (string.IsNullOrEmpty(fieldValue) || string.IsNullOrEmpty(searchTerm))
            {
                return 0;
            }

            // Case-insensitive comparison
            var field = fieldValue.ToLowerInvariant();
            var term = searchTerm.ToLowerInvariant();

            // Exact match gets the highest score
            if (field == term)
            {
                return 100;
            }

            // Contains gets a good score
            if (field.Contains(term))
            {
                return 75;
            }

            // Contains any words gets a lower score
            var words = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (field.Contains(word))
                {
                    return 50;
                }
            }

            return 0;
        }
    }
}