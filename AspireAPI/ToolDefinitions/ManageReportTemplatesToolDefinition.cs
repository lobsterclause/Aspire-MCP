using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading; // Added for CancellationToken
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using NJsonSchema; // Added for JsonSchema
using NJsonSchema.Generation;

namespace AspireAPI.ToolDefinitions
{
    /// <summary>
    /// Tool definition for managing report templates
    /// </summary>
    public class ManageReportTemplatesToolDefinition : IToolDefinition // Ensure IToolDefinition is implemented
    {
        private readonly ILogger<ManageReportTemplatesToolDefinition> _logger;

        public ManageReportTemplatesToolDefinition(ILogger<ManageReportTemplatesToolDefinition> logger)
        {
            _logger = logger;
        }

        public string Name => "manage_report_templates";
        
        public string Description => "Manage report templates including listing, creating, updating, and deleting saved report configurations.";
        
        // Removed RequiresAuthentication as it's not part of IToolDefinition
        // public bool RequiresAuthentication => true;
        
        // Correctly implement the interface method signature
        public Task<JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            // Simplified schema definition using JsonSchemaProperty
            var schema = new JsonSchema
            {
                Type = "object",
                Properties = new Dictionary<string, JsonSchemaProperty>
                {
                    ["action"] = new JsonSchemaProperty {
                        Type = "string",
                        Description = "Action to perform (list, get, create, update, delete)",
                        Enum = new List<string> { "list", "get", "create", "update", "delete" }
                    },
                    ["templateId"] = new JsonSchemaProperty {
                        Type = "string",
                        Description = "Template ID for get, update, or delete actions"
                    },
                    ["template"] = new JsonSchemaProperty {
                        Type = "object",
                        Description = "Template data for create or update actions",
                        Properties = new Dictionary<string, JsonSchemaProperty> {
                            ["name"] = new JsonSchemaProperty { Type = "string", Description = "Name of the template" },
                            ["description"] = new JsonSchemaProperty { Type = "string", Description = "Description of the template" },
                            ["category"] = new JsonSchemaProperty { Type = "string", Description = "Category for organizing templates" },
                            ["definition"] = new JsonSchemaProperty { Type = "object", Description = "Report definition associated with this template" }
                        },
                        Required = new List<string> { "name", "definition" }
                    }
                },
                Required = new List<string> { "action" } // Keep required fields
            };
            
            return Task.FromResult(schema);
        }
    }
}