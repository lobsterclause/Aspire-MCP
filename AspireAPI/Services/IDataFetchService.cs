using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Models;

namespace AspireAPI.Services
{
    /// <summary>
    /// Interface for data fetching service
    /// </summary>
    public interface IDataFetchService
    {
        Task<List<Dictionary<string, object>>> FetchDataAsync(
            DataSource dataSource, 
            CancellationToken cancellationToken = default);

        Task<List<Dictionary<string, object>>> FetchAndJoinDataAsync(
            List<DataSource> dataSources,
            CancellationToken cancellationToken = default);

        Task<ApiResult> FetchDataWithQueryAsync(
            string endpoint, 
            Dictionary<string, string> queryParams, 
            CancellationToken cancellationToken = default);

        Task<SearchResult<PaymentDto>> FetchPaymentsAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task<SearchResult<PropertyDto>> FetchPropertiesAsync(
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task<SearchResult<ContactDto>> FetchContactsAsync(
            string type = "all",
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task<SearchResult<JobDto>> FetchJobsAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Fetches entity data by entity type and optional id
        /// </summary>
        /// <param name="entityType">Type of entity to fetch</param>
        /// <param name="entityId">Optional entity ID</param>
        /// <param name="parameters">Optional additional parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Entity data</returns>
        Task<Dictionary<string, object>> FetchEntityDataAsync(
            string entityType,
            string entityId = null,
            Dictionary<string, string> parameters = null,
            CancellationToken cancellationToken = default);
    }
}