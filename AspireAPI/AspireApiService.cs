using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspireAPI.Models;

namespace AspireAPI
{
    /// <summary>
    /// Service for interacting with the Aspire Cloud API
    /// </summary>
    public class AspireApiService
    {
        private readonly ILogger<AspireApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private readonly AspireApiOptions _options;
        private readonly AspireApiHelpers _apiHelpers;

        public AspireApiService(
            ILogger<AspireApiService> logger,
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IOptions<AspireApiOptions> options,
            AspireApiHelpers apiHelpers)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _options = options.Value;
            _apiHelpers = apiHelpers;
        }

        /// <summary>
        /// Creates an HTTP client with authentication
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>An authenticated HTTP client</returns>
        public HttpClient CreateAuthenticatedClient(string accessToken)
        {
            var client = _httpClientFactory.CreateClient("AspireApi");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return client;
        }

        /// <summary>
        /// Creates an authenticated HTTP client using the TokenService
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An authenticated HTTP client</returns>
        private async Task<HttpClient> CreateAuthenticatedClientAsync(CancellationToken cancellationToken = default)
        {
            var token = await _tokenService.EnsureValidAsync(cancellationToken);
            return CreateAuthenticatedClient(token);
        }

        /// <summary>
        /// Gets a list of contacts
        /// </summary>
        public async Task<SearchResult<ContactDto>> GetContactsAsync(
            string type = "all",
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting contacts of type {type}");
            
            try
            {
                // Create authenticated client
                var client = await CreateAuthenticatedClientAsync(cancellationToken);
                
                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    { "type", type },
                    { "pageNumber", pageNumber.ToString() },
                    { "pageSize", pageSize.ToString() }
                };
                
                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add("search", search);
                }
                
                // Build request URL
                var queryString = _apiHelpers.BuildQueryString(queryParams);
                var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/contacts{queryString}";
                
                // Make the request
                _logger.LogDebug("Making request to {Url}", requestUrl);
                var response = await client.GetAsync(requestUrl, cancellationToken);
                
                // Check for success
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<SearchResult<ContactDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return result ?? new SearchResult<ContactDto>
                    {
                        Data = new List<ContactDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to get contacts: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    
                    return new SearchResult<ContactDto>
                    {
                        Data = new List<ContactDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contacts");
                return new SearchResult<ContactDto>
                {
                    Data = new List<ContactDto>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Gets a list of divisions
        /// </summary>
        public async Task<SearchResult<DivisionDto>> GetDivisionsAsync(
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting divisions");
            
            // Return stub data for now
            return new SearchResult<DivisionDto>
            {
                Data = new List<DivisionDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Gets a list of properties
        /// </summary>
        public async Task<SearchResult<PropertyDto>> GetPropertiesAsync(
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting properties");
            
            try
            {
                // Create authenticated client
                var client = await CreateAuthenticatedClientAsync(cancellationToken);
                
                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    { "pageNumber", pageNumber.ToString() },
                    { "pageSize", pageSize.ToString() }
                };
                
                if (!string.IsNullOrEmpty(contactId))
                {
                    queryParams.Add("contactId", contactId);
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add("search", search);
                }
                
                // Build request URL
                var queryString = _apiHelpers.BuildQueryString(queryParams);
                var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/properties{queryString}";
                
                // Make the request
                _logger.LogDebug("Making request to {Url}", requestUrl);
                var response = await client.GetAsync(requestUrl, cancellationToken);
                
                // Check for success
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<SearchResult<PropertyDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return result ?? new SearchResult<PropertyDto>
                    {
                        Data = new List<PropertyDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to get properties: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    
                    return new SearchResult<PropertyDto>
                    {
                        Data = new List<PropertyDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting properties");
                return new SearchResult<PropertyDto>
                {
                    Data = new List<PropertyDto>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Gets a list of payments
        /// </summary>
        public async Task<SearchResult<PaymentDto>> GetPaymentsAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting payments");
            
            try
            {
                // Create authenticated client
                var client = await CreateAuthenticatedClientAsync(cancellationToken);
                
                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    { "pageNumber", pageNumber.ToString() },
                    { "pageSize", pageSize.ToString() }
                };
                
                if (!string.IsNullOrEmpty(status))
                {
                    queryParams.Add("status", status);
                }
                
                if (!string.IsNullOrEmpty(contactId))
                {
                    queryParams.Add("contactId", contactId);
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add("search", search);
                }
                
                // Build request URL
                var queryString = _apiHelpers.BuildQueryString(queryParams);
                var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/payments{queryString}";
                
                // Make the request
                _logger.LogDebug("Making request to {Url}", requestUrl);
                var response = await client.GetAsync(requestUrl, cancellationToken);
                
                // Check for success
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<SearchResult<PaymentDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return result ?? new SearchResult<PaymentDto>
                    {
                        Data = new List<PaymentDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to get payments: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    
                    return new SearchResult<PaymentDto>
                    {
                        Data = new List<PaymentDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payments");
                return new SearchResult<PaymentDto>
                {
                    Data = new List<PaymentDto>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Gets a list of jobs
        /// </summary>
        public async Task<SearchResult<JobDto>> GetJobsAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting jobs");
            
            try
            {
                // Create authenticated client
                var client = await CreateAuthenticatedClientAsync(cancellationToken);
                
                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    { "pageNumber", pageNumber.ToString() },
                    { "pageSize", pageSize.ToString() }
                };
                
                if (!string.IsNullOrEmpty(status))
                {
                    queryParams.Add("status", status);
                }
                
                if (!string.IsNullOrEmpty(contactId))
                {
                    queryParams.Add("contactId", contactId);
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add("search", search);
                }
                
                // Build request URL
                var queryString = _apiHelpers.BuildQueryString(queryParams);
                var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/jobs{queryString}";
                
                // Make the request
                _logger.LogDebug("Making request to {Url}", requestUrl);
                var response = await client.GetAsync(requestUrl, cancellationToken);
                
                // Check for success
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<SearchResult<JobDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return result ?? new SearchResult<JobDto>
                    {
                        Data = new List<JobDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to get jobs: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    
                    return new SearchResult<JobDto>
                    {
                        Data = new List<JobDto>(),
                        Total = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting jobs");
                return new SearchResult<JobDto>
                {
                    Data = new List<JobDto>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Gets a list of invoices
        /// </summary>
        public async Task<SearchResult<InvoiceDto>> GetInvoicesAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting invoices");
            
            // Return stub data for now
            return new SearchResult<InvoiceDto>
            {
                Data = new List<InvoiceDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        /// <summary>
        /// Gets a list of time entries
        /// </summary>
        public async Task<List<TimeEntryDto>> GetTimeEntriesAsync(
            string startDate = null,
            string endDate = null,
            string employeeId = null,
            string jobId = null,
            string search = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting time entries");
            
            // Return stub data for now
            return new List<TimeEntryDto>();
        }
        
        /// <summary>
        /// Gets a list of branches
        /// </summary>
        public async Task<SearchResult<BranchDto>> GetBranchesAsync(
            string divisionId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting branches");
            
            // Return stub data for now
            return new SearchResult<BranchDto>
            {
                Data = new List<BranchDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        /// <summary>
        /// Gets a list of opportunities
        /// </summary>
        public async Task<SearchResult<OpportunityDto>> GetOpportunitiesAsync(
            string status = null,
            string contactId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting opportunities");
            
            // Return stub data for now
            return new SearchResult<OpportunityDto>
            {
                Data = new List<OpportunityDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        /// <summary>
        /// Gets a list of inventory items
        /// </summary>
        public async Task<SearchResult<InventoryItemDto>> GetInventoryItemsAsync(
            string categoryId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting inventory items");
            
            // Return stub data for now
            return new SearchResult<InventoryItemDto>
            {
                Data = new List<InventoryItemDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        /// <summary>
        /// Gets a list of work tickets
        /// </summary>
        public async Task<SearchResult<WorkTicketDto>> GetWorkTicketsAsync(
            string status = null,
            string jobId = null,
            string search = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting work tickets");
            
            // Return stub data for now
            return new SearchResult<WorkTicketDto>
            {
                Data = new List<WorkTicketDto>(),
                Total = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Gets a contact ID by name
        /// </summary>
        /// <param name="contactType">Type of contact (e.g., customer, vendor)</param>
        /// <param name="contactName">Name of the contact</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Contact ID or null if not found</returns>
        public async Task<string> GetContactIdByNameAsync(
            string contactType,
            string contactName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Looking up contact ID for {contactName}");
            
            try
            {
                // Search for contacts by name
                var contacts = await GetContactsAsync(
                    type: contactType,
                    search: contactName,
                    pageSize: 1,
                    cancellationToken: cancellationToken);
                
                // Return the ID of the first match, if any
                return contacts.Data.Count > 0 ? contacts.Data[0].Id : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up contact ID for {contactName}");
                return null;
            }
        }

        /// <summary>
        /// Gets a division ID by name
        /// </summary>
        /// <param name="divisionName">Name of the division</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Division ID or null if not found</returns>
        public async Task<string> GetDivisionIdByNameAsync(
            string divisionName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Looking up division ID for {divisionName}");
            
            try
            {
                // Search for divisions by name
                var divisions = await GetDivisionsAsync(
                    search: divisionName,
                    pageSize: 1,
                    cancellationToken: cancellationToken);
                
                // Return the ID of the first match, if any
                return divisions.Data.Count > 0 ? divisions.Data[0].Id : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up division ID for {divisionName}");
                return null;
            }
        }
    }
}
