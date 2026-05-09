using ModelContextProtocol.Protocol.Types;
using NJsonSchema;  // Add reference to NJsonSchema
using NJsonSchema.Generation;  // Add reference to NJsonSchema.Generation
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI.ToolDefinitions
{
    // Property input class for create/update operations
    public class CreateUpdatePropertyInput
    {
        public string? PropertyId { get; set; } // Optional: If provided, updates existing property
        public string PropertyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., For Sale, Sold, Pending
        public string PropertyType { get; set; } = string.Empty; // e.g., Residential, Commercial
        public int SquareFootage { get; set; }
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? YearBuilt { get; set; }
        public string? Description { get; set; }
        public string? OwnerId { get; set; } // Optional: Link to Contact
        public string? LocationId { get; set; } // Optional: Link to Location/Branch
        public string? SalesAgentId { get; set; } // Optional: Link to Contact (Agent)
    }

    // Implement the tool definition
    public class CreateUpdatePropertyToolDefinition : IToolDefinition
    {
        public string Name => "CreateUpdateProperty"; // Keep name for potential reference
        public string Description => "STUB - Creates or updates a property record.";

        public async Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromTypeAsync<CreateUpdatePropertyInput>(new JsonSchemaGeneratorSettings { GenerateExamples = true });
        }
    }
}