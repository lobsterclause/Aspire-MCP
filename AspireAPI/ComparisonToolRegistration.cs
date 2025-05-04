using ModelContextProtocol.Protocol.Types;
using System.Collections.Generic;
using AspireAPI.Models; // Added using directive for Models

namespace AspireAPI
{
    /// <summary>
    /// Extension methods for registering the Comparison Tool
    /// </summary>
    public static class ComparisonToolRegistration
    {
        /// <summary>
        /// Registers the Comparison Tool with the MCP server
        /// </summary>
        public static void RegisterComparisonTool(this List<Tool> tools)
        {
            // Add the CompareData tool
            tools.Add(new Tool
            {
                Name = "CompareData",
                Description = "Compare data between different time periods, entities, or branches",
                InputSchema = new JsonSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["entityType"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Type of entity to compare",
                            Enum = new[]
                            {
                                "TimeEntries",
                                "Contacts",
                                "Invoices",
                                "Opportunities",
                                "Jobs"
                            }
                        },
                        ["dimension"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Dimension to compare across",
                            Enum = new[]
                            {
                                "time",
                                "branch",
                                "division",
                                "employee",
                                "client"
                            }
                        },
                        ["firstValue"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "First value for comparison (date range, branch ID, etc.)"
                        },
                        ["secondValue"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Second value for comparison (date range, branch ID, etc.)"
                        },
                        ["firstStartDate"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Start date for first time period (required if dimension is 'time' and firstValue is 'custom')"
                        },
                        ["firstEndDate"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "End date for first time period (required if dimension is 'time' and firstValue is 'custom')"
                        },
                        ["secondStartDate"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Start date for second time period (required if dimension is 'time' and secondValue is 'custom')"
                        },
                        ["secondEndDate"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "End date for second time period (required if dimension is 'time' and secondValue is 'custom')"
                        },
                        ["metrics"] = new JsonSchemaProperty
                        {
                            Type = "array",
                            Description = "Metrics to calculate and compare",
                            Items = new JsonSchemaProperty
                            {
                                Type = "string",
                                Enum = new[]
                                {
                                    "totalHours",
                                    "totalCost",
                                    "averageHours",
                                    "averageCost",
                                    "totalRevenue",
                                    "totalProfit",
                                    "billableHours",
                                    "nonBillableHours",
                                    "utilizationRate",
                                    "averageHourlyRate",
                                    "profitMargin",
                                    "count"
                                }
                            },
                            Default = new[] { "totalHours", "totalCost" }
                        },
                        ["filters"] = new JsonSchemaProperty
                        {
                            Type = "array",
                            Description = "Additional filters to apply to both datasets",
                            Items = new JsonSchemaProperty
                            {
                                Type = "object",
                                Properties = new Dictionary<string, JsonSchemaProperty>
                                {
                                    ["field"] = new JsonSchemaProperty { Type = "string", Description = "Field name" },
                                    ["operator"] = new JsonSchemaProperty
                                    {
                                        Type = "string",
                                        Description = "Comparison operator",
                                        Enum = new[] { "eq", "neq", "gt", "lt", "gte", "lte", "contains" }
                                    },
                                    ["value"] = new JsonSchemaProperty { Type = "string", Description = "Value to compare against" }
                                },
                                Required = new[] { "field", "operator", "value" }
                            }
                        },
                        ["groupBy"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Field to group comparison by (leave empty for no grouping)"
                        }
                    },
                    Required = new[] { "entityType", "dimension", "firstValue", "secondValue" }
                }
            });
        }
    }
}