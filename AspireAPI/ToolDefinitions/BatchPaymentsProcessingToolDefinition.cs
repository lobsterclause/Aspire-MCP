using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    public class BatchPaymentsProcessingToolDefinition : IToolDefinition
    {
        public string Name => "BatchPaymentsProcessing";
        public string Description => "Process multiple payments in a single batch operation with transactional integrity";

        /// <summary>
        /// Input model for BatchPaymentsProcessing tool
        /// </summary>
        public class BatchPaymentsProcessingInput
        {
            [JsonPropertyName("operation")]
            [Description("Batch operation type")]
            [Required]
            public string Operation { get; set; }
            
            [JsonPropertyName("payments")]
            [Description("Array of payment objects to process")]
            [Required]
            public List<PaymentItem> Payments { get; set; }
            
            [JsonPropertyName("rollbackOnError")]
            [Description("Whether to roll back all operations if any operation fails (default: true)")]
            public bool? RollbackOnError { get; set; }
            
            [JsonPropertyName("statusUpdate")]
            [Description("Status update parameters (required for statusUpdate operation)")]
            public StatusUpdateParams StatusUpdate { get; set; }
            
            public class PaymentItem
            {
                [JsonPropertyName("id")]
                [Description("Payment ID (required for update/delete operations)")]
                public string Id { get; set; }
                
                [JsonPropertyName("amount")]
                [Description("Payment amount")]
                public decimal? Amount { get; set; }
                
                [JsonPropertyName("date")]
                [Description("Payment date (ISO format)")]
                public string Date { get; set; }
                
                [JsonPropertyName("status")]
                [Description("Payment status")]
                public string Status { get; set; }
                
                [JsonPropertyName("paymentMethodId")]
                [Description("Payment method ID")]
                public string PaymentMethodId { get; set; }
                
                [JsonPropertyName("invoiceId")]
                [Description("Associated invoice ID")]
                public string InvoiceId { get; set; }
                
                [JsonPropertyName("contactId")]
                [Description("Associated contact ID")]
                public string ContactId { get; set; }
            }
            
            public class StatusUpdateParams
            {
                [JsonPropertyName("fromStatus")]
                [Description("Current status to match")]
                public string FromStatus { get; set; }
                
                [JsonPropertyName("toStatus")]
                [Description("New status to set")]
                public string ToStatus { get; set; }
            }
        }

        public async Task<JsonSchema> GetSchemaAsync(IServiceProvider serviceProvider)
        {
            return await JsonSchema.FromTypeAsync<BatchPaymentsProcessingInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}