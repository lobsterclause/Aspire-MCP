using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Models;
using AspireAPI.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace AspireAPI.Handlers
{
    /// <summary>
    /// Handler for the generate_report tool
    /// </summary>
    public class GenerateReportHandler : BaseHandler
    {
        private readonly ReportService _reportService;
        private new readonly ILogger<GenerateReportHandler> _logger;

        public GenerateReportHandler(
            ILogger<GenerateReportHandler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            ReportService reportService)
            : base(logger, httpClientFactory, apiHelpers)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the generate_report tool request
        /// </summary>
        public override async Task<CallToolResponse> HandleAsync(
            IDictionary<string, object> arguments,
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling generate_report request");

                // Convert dictionary to JsonElement for compatibility with existing code
                var jsonElement = ConvertArgumentsToJsonElement(arguments);

                // Convert the request parameters to a ReportDefinition
                var reportDefinition = ParseReportDefinition(jsonElement);

                // Generate and format the report
                var formattedReport = await _reportService.GenerateFormattedReportAsync(
                    reportDefinition, cancellationToken);

                // Create response
                var response = JsonSerializer.Serialize(new 
                {
                    reportName = formattedReport.ReportName,
                    format = formattedReport.Format,
                    generatedAt = formattedReport.GeneratedAt,
                    filename = formattedReport.Filename,
                    contentType = formattedReport.ContentType,
                    size = formattedReport.Data.Length,
                    data = Convert.ToBase64String(formattedReport.Data)
                });

                return new CallToolResponse
                {
                    Content = new[]
                    {
                        new Content
                        {
                            Text = response
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling generate_report request");
                throw new McpServerException($"Error generating report: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts dictionary arguments to JsonElement
        /// </summary>
        private JsonElement ConvertArgumentsToJsonElement(IDictionary<string, object> arguments)
        {
            // Serialize and deserialize to convert from dictionary to JsonElement
            var json = JsonSerializer.Serialize(arguments);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        /// <summary>
        /// Parses the JSON parameters into a ReportDefinition object
        /// </summary>
        private ReportDefinition ParseReportDefinition(JsonElement parameters)
        {
            var reportDefinition = new ReportDefinition
            {
                Name = GetStringProperty(parameters, "name"),
                Description = GetStringProperty(parameters, "description"),
                TemplateId = GetStringProperty(parameters, "templateId"),
                OutputFormat = GetStringProperty(parameters, "outputFormat", "json"),
                PageNumber = GetIntProperty(parameters, "pageNumber", 1),
                PageSize = GetIntProperty(parameters, "pageSize", 50)
            };

            // Parse data sources
            if (parameters.TryGetProperty("dataSources", out var dataSources) && dataSources.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.DataSources = new List<DataSource>();
                foreach (var source in dataSources.EnumerateArray())
                {
                    reportDefinition.DataSources.Add(ParseDataSource(source));
                }
            }

            // Parse columns
            if (parameters.TryGetProperty("columns", out var columns) && columns.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.Columns = new List<string>();
                foreach (var column in columns.EnumerateArray())
                {
                    if (column.ValueKind == JsonValueKind.String)
                    {
                        reportDefinition.Columns.Add(column.GetString());
                    }
                }
            }

            // Parse filters
            if (parameters.TryGetProperty("filters", out var filters) && filters.ValueKind == JsonValueKind.Object)
            {
                reportDefinition.Filters = ParseFilterGroup(filters);
            }

            // Parse calculations
            if (parameters.TryGetProperty("calculations", out var calculations) && calculations.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.Calculations = new List<CalculationDefinition>();
                foreach (var calc in calculations.EnumerateArray())
                {
                    reportDefinition.Calculations.Add(ParseCalculation(calc));
                }
            }

            // Parse groupBy
            if (parameters.TryGetProperty("groupBy", out var groupBy) && groupBy.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.GroupBy = new List<string>();
                foreach (var field in groupBy.EnumerateArray())
                {
                    if (field.ValueKind == JsonValueKind.String)
                    {
                        reportDefinition.GroupBy.Add(field.GetString());
                    }
                }
            }

            // Parse aggregations
            if (parameters.TryGetProperty("aggregations", out var aggregations) && aggregations.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.Aggregations = new List<AggregationDefinition>();
                foreach (var agg in aggregations.EnumerateArray())
                {
                    reportDefinition.Aggregations.Add(ParseAggregation(agg));
                }
            }

            // Parse sortBy
            if (parameters.TryGetProperty("sortBy", out var sortBy) && sortBy.ValueKind == JsonValueKind.Array)
            {
                reportDefinition.SortBy = new List<SortDefinition>();
                foreach (var sort in sortBy.EnumerateArray())
                {
                    reportDefinition.SortBy.Add(ParseSortDefinition(sort));
                }
            }

            // Parse visualization
            if (parameters.TryGetProperty("visualization", out var visualization) && visualization.ValueKind == JsonValueKind.Object)
            {
                reportDefinition.Visualization = ParseVisualization(visualization);
            }

            return reportDefinition;
        }

        /// <summary>
        /// Parses a data source from JSON
        /// </summary>
        private DataSource ParseDataSource(JsonElement element)
        {
            var dataSource = new DataSource
            {
                EntityType = GetStringProperty(element, "entityType"),
                IsPrimary = GetBoolProperty(element, "isPrimary", false),
                JoinType = GetStringProperty(element, "joinType"),
                RightPrefix = GetStringProperty(element, "rightPrefix")
            };

            // Parse parameters
            if (element.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Object)
            {
                dataSource.Parameters = new Dictionary<string, object>();
                foreach (var param in parameters.EnumerateObject())
                {
                    dataSource.Parameters[param.Name] = JsonSerializer.Deserialize<object>(param.Value.GetRawText());
                }
            }

            // Parse join condition
            if (element.TryGetProperty("joinCondition", out var joinCondition) && joinCondition.ValueKind == JsonValueKind.Object)
            {
                dataSource.JoinCondition = new JoinCondition
                {
                    LeftField = GetStringProperty(joinCondition, "leftField"),
                    RightField = GetStringProperty(joinCondition, "rightField"),
                    ComparisonOperator = GetStringProperty(joinCondition, "comparisonOperator", "eq")
                };
            }

            return dataSource;
        }

        /// <summary>
        /// Parses a filter group from JSON
        /// </summary>
        private FilterGroup ParseFilterGroup(JsonElement element)
        {
            var filterGroup = new FilterGroup
            {
                LogicalOperator = GetStringProperty(element, "logicalOperator", "and")
            };

            // Parse conditions
            if (element.TryGetProperty("conditions", out var conditions) && conditions.ValueKind == JsonValueKind.Array)
            {
                filterGroup.Conditions = new List<FilterCondition>();
                foreach (var condition in conditions.EnumerateArray())
                {
                    filterGroup.Conditions.Add(ParseFilterCondition(condition));
                }
            }

            // Parse nested groups
            if (element.TryGetProperty("groups", out var groups) && groups.ValueKind == JsonValueKind.Array)
            {
                filterGroup.Groups = new List<FilterGroup>();
                foreach (var group in groups.EnumerateArray())
                {
                    filterGroup.Groups.Add(ParseFilterGroup(group));
                }
            }

            return filterGroup;
        }

        /// <summary>
        /// Parses a filter condition from JSON
        /// </summary>
        private FilterCondition ParseFilterCondition(JsonElement element)
        {
            var condition = new FilterCondition
            {
                Field = GetStringProperty(element, "field"),
                Operator = GetStringProperty(element, "operator", "eq")
            };

            // Parse value
            if (element.TryGetProperty("value", out var value))
            {
                condition.Value = JsonSerializer.Deserialize<object>(value.GetRawText());
            }

            // Parse values array
            if (element.TryGetProperty("values", out var values) && values.ValueKind == JsonValueKind.Array)
            {
                condition.Values = new List<object>();
                foreach (var val in values.EnumerateArray())
                {
                    condition.Values.Add(JsonSerializer.Deserialize<object>(val.GetRawText()));
                }
            }

            return condition;
        }

        /// <summary>
        /// Parses a calculation definition from JSON
        /// </summary>
        private CalculationDefinition ParseCalculation(JsonElement element)
        {
            var calculation = new CalculationDefinition
            {
                Name = GetStringProperty(element, "name"),
                Type = GetStringProperty(element, "type"),
                Formula = GetStringProperty(element, "formula"),
                DerivedFrom = GetStringProperty(element, "derivedFrom"),
                CustomFunction = GetStringProperty(element, "customFunction")
            };

            // Parse parameters
            if (element.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Object)
            {
                calculation.Parameters = new Dictionary<string, object>();
                foreach (var param in parameters.EnumerateObject())
                {
                    calculation.Parameters[param.Name] = JsonSerializer.Deserialize<object>(param.Value.GetRawText());
                }
            }

            return calculation;
        }

        /// <summary>
        /// Parses an aggregation definition from JSON
        /// </summary>
        private AggregationDefinition ParseAggregation(JsonElement element)
        {
            var aggregation = new AggregationDefinition
            {
                Name = GetStringProperty(element, "name"),
                Function = GetStringProperty(element, "function"),
                Field = GetStringProperty(element, "field")
            };

            // Parse percentile value
            if (element.TryGetProperty("percentile", out var percentile) && percentile.ValueKind == JsonValueKind.Number)
            {
                aggregation.Percentile = percentile.GetDecimal();
            }

            return aggregation;
        }

        /// <summary>
        /// Parses a sort definition from JSON
        /// </summary>
        private SortDefinition ParseSortDefinition(JsonElement element)
        {
            return new SortDefinition
            {
                Field = GetStringProperty(element, "field"),
                Direction = GetStringProperty(element, "direction", "asc")
            };
        }

        /// <summary>
        /// Parses a visualization specification from JSON
        /// </summary>
        private VisualizationSpec ParseVisualization(JsonElement element)
        {
            var visualization = new VisualizationSpec
            {
                Type = GetStringProperty(element, "type"),
                Title = GetStringProperty(element, "title"),
                Subtitle = GetStringProperty(element, "subtitle")
            };

            // Parse options
            if (element.TryGetProperty("options", out var options) && options.ValueKind == JsonValueKind.Object)
            {
                visualization.Options = new Dictionary<string, object>();
                foreach (var option in options.EnumerateObject())
                {
                    visualization.Options[option.Name] = JsonSerializer.Deserialize<object>(option.Value.GetRawText());
                }
            }

            // Parse series
            if (element.TryGetProperty("series", out var series) && series.ValueKind == JsonValueKind.Array)
            {
                visualization.Series = new List<VisualizationSeries>();
                foreach (var s in series.EnumerateArray())
                {
                    visualization.Series.Add(new VisualizationSeries
                    {
                        Name = GetStringProperty(s, "name"),
                        Field = GetStringProperty(s, "field"),
                        Type = GetStringProperty(s, "type"),
                        YAxisIndex = GetStringProperty(s, "yAxisIndex", "0"),
                        Style = ParseStyleDictionary(s.GetProperty("style", default))
                    });
                }
            }

            // Parse axis
            if (element.TryGetProperty("xAxis", out var xAxis) && xAxis.ValueKind == JsonValueKind.Object)
            {
                visualization.XAxis = new VisualizationAxis
                {
                    Title = GetStringProperty(xAxis, "title"),
                    Type = GetStringProperty(xAxis, "type", "category"),
                    ShowGrid = GetBoolProperty(xAxis, "showGrid", true),
                    Style = ParseStyleDictionary(xAxis.GetProperty("style", default))
                };
            }

            if (element.TryGetProperty("yAxis", out var yAxis) && yAxis.ValueKind == JsonValueKind.Object)
            {
                visualization.YAxis = new VisualizationAxis
                {
                    Title = GetStringProperty(yAxis, "title"),
                    Type = GetStringProperty(yAxis, "type", "value"),
                    ShowGrid = GetBoolProperty(yAxis, "showGrid", true),
                    Style = ParseStyleDictionary(yAxis.GetProperty("style", default))
                };
            }

            // Parse colors
            if (element.TryGetProperty("colors", out var colors) && colors.ValueKind == JsonValueKind.Array)
            {
                visualization.Colors = new List<string>();
                foreach (var color in colors.EnumerateArray())
                {
                    if (color.ValueKind == JsonValueKind.String)
                    {
                        visualization.Colors.Add(color.GetString());
                    }
                }
            }

            // Parse legend
            if (element.TryGetProperty("legend", out var legend) && legend.ValueKind == JsonValueKind.Object)
            {
                visualization.Legend = new Dictionary<string, object>();
                foreach (var property in legend.EnumerateObject())
                {
                    visualization.Legend[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            return visualization;
        }

        /// <summary>
        /// Parses a style dictionary from JSON
        /// </summary>
        private Dictionary<string, object> ParseStyleDictionary(JsonElement element)
        {
            var dictionary = new Dictionary<string, object>();
            
            if (element.ValueKind != JsonValueKind.Object)
            {
                return dictionary;
            }
            
            foreach (var property in element.EnumerateObject())
            {
                dictionary[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
            }
            
            return dictionary;
        }

        /// <summary>
        /// Gets a string property from a JSON element
        /// </summary>
        private string GetStringProperty(JsonElement element, string propertyName, string defaultValue = null)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer property from a JSON element
        /// </summary>
        private int GetIntProperty(JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean property from a JSON element
        /// </summary>
        private bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.True)
            {
                return true;
            }
            if (element.TryGetProperty(propertyName, out property) && property.ValueKind == JsonValueKind.False)
            {
                return false;
            }
            return defaultValue;
        }
    }
}