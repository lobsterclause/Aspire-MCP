using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using AspireAPI.Models;
using AspireAPI.Services;

namespace AspireAPI.Handlers
{
    public class BatchPaymentsProcessingHandler : BaseHandler
    {
        private readonly CacheManager _cacheManager;

        public BatchPaymentsProcessingHandler(
            ILogger<BatchPaymentsProcessingHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            CacheManager cacheManager)
            : base(logger, httpClientFactory, apiHelpers)
        {
            _cacheManager = cacheManager;
        }

        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate and extract required parameters
                if (!arguments.TryGetValue("operation", out var operationObj) || operationObj == null)
                {
                    throw new McpServerException("Operation parameter is required");
                }

                if (!arguments.TryGetValue("payments", out var paymentsObj) || paymentsObj == null)
                {
                    throw new McpServerException("Payments parameter is required");
                }

                string operation = operationObj.ToString().ToLowerInvariant();
                bool rollbackOnError = true;

                // Check if rollbackOnError is specified
                if (arguments.TryGetValue("rollbackOnError", out var rollbackObj) && rollbackObj != null)
                {
                    if (bool.TryParse(rollbackObj.ToString(), out var parsedRollback))
                    {
                        rollbackOnError = parsedRollback;
                    }
                }

                // Convert payments object to list
                var paymentsList = new List<Dictionary<string, object>>();
                try
                {
                    // Serialize and deserialize to convert to expected format
                    var paymentsJson = JsonSerializer.Serialize(paymentsObj);
                    paymentsList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(paymentsJson);
                }
                catch (Exception ex)
                {
                    throw new McpServerException($"Invalid payments data format: {ex.Message}");
                }

                if (paymentsList == null || !paymentsList.Any())
                {
                    throw new McpServerException("No payments provided for batch processing");
                }

                // Process based on operation type
                string result;
                switch (operation)
                {
                    case "create":
                        result = await CreatePaymentsInBatchAsync(paymentsList, rollbackOnError, accessToken, cancellationToken);
                        break;

                    case "update":
                        result = await UpdatePaymentsInBatchAsync(paymentsList, rollbackOnError, accessToken, cancellationToken);
                        break;

                    case "delete":
                        result = await DeletePaymentsInBatchAsync(paymentsList, rollbackOnError, accessToken, cancellationToken);
                        break;

                    case "statusupdate":
                        // Extract status update parameters
                        if (!arguments.TryGetValue("statusUpdate", out var statusUpdateObj) || statusUpdateObj == null)
                        {
                            throw new McpServerException("statusUpdate parameter is required for statusUpdate operation");
                        }

                        try
                        {
                            var statusUpdateJson = JsonSerializer.Serialize(statusUpdateObj);
                            var statusUpdate = JsonSerializer.Deserialize<Dictionary<string, object>>(statusUpdateJson);

                            if (!statusUpdate.TryGetValue("fromStatus", out var fromStatusObj) || fromStatusObj == null ||
                                !statusUpdate.TryGetValue("toStatus", out var toStatusObj) || toStatusObj == null)
                            {
                                throw new McpServerException("fromStatus and toStatus are required in statusUpdate");
                            }

                            result = await UpdatePaymentStatusesInBatchAsync(
                                paymentsList,
                                fromStatusObj.ToString(),
                                toStatusObj.ToString(),
                                rollbackOnError,
                                accessToken,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            throw new McpServerException($"Invalid statusUpdate format: {ex.Message}");
                        }
                        break;

                    default:
                        throw new McpServerException($"Unsupported operation: {operation}");
                }

                // Invalidate cache after batch operations
                await _cacheManager.InvalidateEntityAndRelatedCacheAsync("payments", cancellationToken);

                return CreateResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BatchPaymentsProcessingHandler");
                throw new McpServerException($"Error processing batch payments: {ex.Message}", ex);
            }
        }

        private async Task<string> CreatePaymentsInBatchAsync(
            List<Dictionary<string, object>> payments,
            bool rollbackOnError,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Payments/batch";
            
            var requestData = new
            {
                payments = payments,
                rollbackOnError = rollbackOnError
            };

            _logger.LogInformation($"Creating {payments.Count} payments in batch with rollbackOnError={rollbackOnError}");
            
            var response = await client.PostAsJsonAsync(url, requestData, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }

        private async Task<string> UpdatePaymentsInBatchAsync(
            List<Dictionary<string, object>> payments,
            bool rollbackOnError,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Payments/batch";
            
            var requestData = new
            {
                payments = payments,
                rollbackOnError = rollbackOnError
            };

            _logger.LogInformation($"Updating {payments.Count} payments in batch with rollbackOnError={rollbackOnError}");
            
            var response = await client.PutAsJsonAsync(url, requestData, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }

        private async Task<string> DeletePaymentsInBatchAsync(
            List<Dictionary<string, object>> payments,
            bool rollbackOnError,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Payments/batch/delete";
            
            // For delete, we only need the IDs
            var paymentIds = payments
                .Where(p => p.ContainsKey("id") && p["id"] != null)
                .Select(p => p["id"].ToString())
                .ToList();
            
            if (!paymentIds.Any())
            {
                throw new McpServerException("No valid payment IDs provided for delete operation");
            }
            
            var requestData = new
            {
                paymentIds = paymentIds,
                rollbackOnError = rollbackOnError
            };

            _logger.LogInformation($"Deleting {paymentIds.Count} payments in batch with rollbackOnError={rollbackOnError}");
            
            var response = await client.PostAsJsonAsync(url, requestData, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }

        private async Task<string> UpdatePaymentStatusesInBatchAsync(
            List<Dictionary<string, object>> payments,
            string fromStatus,
            string toStatus,
            bool rollbackOnError,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var client = CreateAuthenticatedClient(accessToken);
            var url = "https://cloud-api.youraspire.com/Payments/batch/status";
            
            // For status update, we only need the IDs
            var paymentIds = payments
                .Where(p => p.ContainsKey("id") && p["id"] != null)
                .Select(p => p["id"].ToString())
                .ToList();
            
            if (!paymentIds.Any())
            {
                throw new McpServerException("No valid payment IDs provided for status update operation");
            }
            
            var requestData = new
            {
                paymentIds = paymentIds,
                fromStatus = fromStatus,
                toStatus = toStatus,
                rollbackOnError = rollbackOnError
            };

            _logger.LogInformation($"Updating status of {paymentIds.Count} payments from '{fromStatus}' to '{toStatus}' with rollbackOnError={rollbackOnError}");
            
            var response = await client.PostAsJsonAsync(url, requestData, cancellationToken);
            return await GetResponseContentAsync(response, cancellationToken);
        }
    }
}