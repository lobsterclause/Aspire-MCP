## Codebase Summary

## Key Components and Their Interactions
- **AppHost:** The .NET Aspire orchestration project. Responsible for setting up the application's infrastructure, including the AspireAPI project and potentially other resources like databases or message queues in the future.
- **AspireAPI:** The core MCP server implementation. This contains the logic for connecting to the Aspire Cloud API, implementing the MCP tools, and handling requests from AI models. Key components include:
  - **AspireMcpServer:** The main MCP server class that initializes the server and registers tools.
  - **AspireToolRouter:** Routes tool requests to the appropriate handlers using a registry pattern.
  - **BaseHandler:** Abstract base class for all specialized handlers, providing common functionality.
  - **Specialized Handlers:** Individual handler classes for each tool (GetTimeEntryReportHandler, ListContactsHandler, etc.) that inherit from BaseHandler.
  - **TokenService:** Handles authentication with the Aspire Cloud API.
  - **Tool Definitions:** Classes defining the schema and metadata for each tool.
  - **CachingService:** Service for caching API responses to improve performance and reduce API calls.
  - **ComparisonService:** Service for comparing data across different dimensions.
  - **MetricCalculationService:** Service for calculating various business metrics.
  - **DataExportService:** Service for exporting data in various formats.
  - **DataProcessingService:** Service for applying filters, calculations, grouping, and sorting to data.
  - **EntityConverterService:** Service for converting entity objects to dictionaries.
  - **TrendService:** Service for analyzing historical trends.
  - **RelationshipFinders:** Classes for finding relationships between entities.
  - **EntitySearchers:** Classes for searching across different entity types.
  - **WorkflowStatusService:** Service for tracking entities through workflow stages.
  - **ReportService:** Advanced reporting service supporting multi-entity reports, custom calculations, and visualization.
  - **ReportTemplateService:** Service for managing saved report templates.
  - **ReportVisualizationService:** Service for generating visualizations from report data.
  - **ReportOutputService:** Service for formatting reports in different output formats (JSON, CSV, Excel, PDF, HTML).
  - **AdvancedFilterService:** Service for complex, nested filtering with AND/OR logic.
  - **Trend Models (in AspireAPI.Trend namespace):** Data models for trend analysis results and parameters.
  - **Workflow Models (in AspireAPI.Workflow namespace):** Data models for workflow status tracking.
  - **Report Models (in AspireAPI.Models namespace):** Data models for advanced report generation and templates.

## Data Flow
AI Model -> MCP Server (AspireAPI) -> AspireToolRouter -> Specialized Handler -> Aspire Cloud API -> Handler (process data) -> AI Model

## External Dependencies
- **Aspire Cloud API:** The primary external dependency for business data and functionality. Authentication is handled via environment variables (`ASPIRE__BASE_URL`, `ASPIRE__USERNAME`, `ASPIRE__PASSWORD`, `ASPIRE__COMPANYKEY`).
- **Microsoft.Extensions.ServiceDiscovery:** Used by Aspire for discovering and connecting to services.
- **Microsoft.Extensions.Hosting:** Provides the hosting environment for the application.
- **Microsoft.Extensions.Configuration:** Used for handling configuration, including environment variables.
- **Microsoft.Extensions.DependencyInjection:** For managing dependencies within the AspireAPI project.
- **Microsoft.AspNetCore.OpenApi:** For generating OpenAPI specifications for the API.
- **Swashbuckle.AspNetCore:** For integrating Swagger UI for API documentation and testing.
- **Microsoft.AspNetCore.Mvc:** For building the API endpoints.
- **Microsoft.AspNetCore.Routing:** For handling request routing.
- **Microsoft.AspNetCore.Server.Kestrel:** The default web server for ASP.NET Core.
- **Microsoft.Extensions.Logging:** For logging within the application.
- **System.Net.Http:** For making HTTP requests to the Aspire Cloud API.
- **System.Text.Json:** For handling JSON serialization and deserialization.

## Recent Significant Changes
- Initial project structure documentation created.
- Comprehensive refactoring to implement a clean architecture pattern.
- Implemented new services for data comparison, metric calculation, data export, data processing, entity conversion, trend analysis, relationship finding, entity searching, and workflow status tracking.
- Created new data models for trend analysis and workflow status tracking.
- Integrated new tools into the MCP server for these functionalities.
- Ensured code adheres to line count limits (max 200 lines per file).
- Applied consistent error handling and logging patterns across handlers.
- Implemented Time-Entry Report Grouping and Date Range Shortcuts.
- **Implemented caching mechanism** to improve performance:
  - Added a new `CachingService` to manage caching of API responses
  - Implemented parameterized cache keys based on request parameters
  - Set appropriate cache durations for different entity types
  - Configured cache invalidation for data freshness
  - Applied caching to all major data fetching operations
- **Implemented advanced reporting system**:
  - Created a comprehensive ReportService with support for multi-entity reports
  - Implemented report templates with save/load capabilities
  - Added support for complex, nested filtering with AND/OR logic
  - Developed a visualization system for charts and graphs
  - Added support for various output formats (JSON, CSV, Excel, PDF, HTML)
  - Created calculated fields and formula support
  - Implemented data aggregation and grouping functionality
  - Developed custom sorting and pagination

## User Feedback Integration and Its Impact on Development
(No user feedback received yet)

## Additional Documentation
- `cline_docs/vision.md`: Project vision and goals.
- `cline_docs/projectRoadmap.md`: High-level roadmap and progress.
- `cline_docs/currentTask.md`: Details of the current task.
- `cline_docs/techStack.md`: Overview of the technology stack.
- `cline_docs/wayOfWorking.md`: Development processes and standards.
- `cline_docs/definitionOfDone.md`: Criteria for task completion.