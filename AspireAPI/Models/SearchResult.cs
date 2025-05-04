using System;
using System.Collections.Generic;

namespace AspireAPI.Models
{
    /// <summary>
    /// Individual search result entity for entity searching
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of entity (e.g. "Contacts", "Jobs", etc.)
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Title/name of the entity for display
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the entity for display
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Last modified date of the entity
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// URL to access the entity
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Score indicating how well the entity matches search criteria
        /// </summary>
        public int MatchScore { get; set; }

        /// <summary>
        /// Additional entity-specific data
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Search query that was used
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Collection of search results
        /// </summary>
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

        /// <summary>
        /// Total count of results
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }
    }

    /// <summary>
    /// Generic search result container
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class SearchResult<T>
    {
        /// <summary>
        /// List of items
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();
        
        /// <summary>
        /// Total number of items available
        /// </summary>
        public int Total { get; set; }
        
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
    }
}