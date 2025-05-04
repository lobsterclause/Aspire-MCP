# Aspire MCP Server Testing Strategy

## 1. Introduction

This document outlines the comprehensive testing strategy for the Aspire MCP server project. The goal is to ensure the reliability, correctness, and robustness of the MCP server and its interactions with the Aspire Cloud API. This strategy aligns with the project's `definitionOfDone.md` and `wayOfWorking.md`.

## 2. Testing Levels

We will employ a multi-layered testing approach:

### 2.1. Unit Testing

*   **Scope:** Individual C# classes and methods, focusing on isolated components like specific `Handlers` (e.g., `ListContactsHandler`, `ListPropertiesHandler`, `ListEquipmentHandler`, `CreateUpdatePropertyHandler`) and `Services` (e.g., `DataProcessingService`, `TrendService`, `AdvancedCachingService`, `CacheManager`).
*   **Tools:** Standard .NET testing frameworks (e.g., xUnit, MSTest). Mocking libraries (e.g., Moq, NSubstitute) will be used to isolate components under test.
*   **Focus:** Verifying business logic, input validation, calculations, error handling within components, and adherence to single responsibility principles. Dependencies (like `AspireApiService` or other services) will be mocked.
    *   **For Handlers (ListProperties, ListEquipment, CreateUpdateProperty):** Test parsing of input parameters, calling the underlying service (mocked), and mapping the result to the output format. For `CreateUpdatePropertyHandler`, specifically test input validation and correct data passed to the mocked service.
    *   **For Caching Infrastructure (AdvancedCachingService, CacheManager):** Test cache hit/miss logic, cache expiration, cache invalidation scenarios, and concurrent access handling (if applicable). Mock the data fetching service to control cache behavior.
*   **Automation:** Fully automated and run frequently during development and potentially as part of a CI pipeline.

### 2.2. Integration Testing

*   **Scope:** Testing the interaction between different components of the MCP server, particularly the interaction between services like `AspireApiService` and `TokenService` with the actual Aspire Cloud API, and the interaction of handlers with the caching layer and API.
*   **Environment:** Primarily utilizes the **Aspire Sandbox Environment**. Requires careful configuration management (environment variables) to point to the sandbox endpoints and credentials.
*   **Focus:** Validating authentication flows (`TokenService`), verifying the correctness of API calls made by `AspireApiService`, ensuring data contracts match the Aspire Cloud API, and testing data transformations between the API and MCP server models.
    *   **Handler Integration:** Test `ListPropertiesHandler` and `ListEquipmentHandler` against the Aspire Sandbox API to ensure correct data fetching and mapping. Test `CreateUpdatePropertyHandler` against the Aspire Sandbox API to verify property creation/update functionality end-to-end within the server context.
    *   **Caching Integration:** Test the interaction between handlers and the caching layer. Verify that subsequent calls to handlers for the same data result in a cache hit (after the first call). Test cache invalidation scenarios (e.g., after a property is updated via `CreateUpdatePropertyHandler`, subsequent `ListPropertiesHandler` calls should fetch fresh data or the cache should be updated). Test cache expiration behavior.
*   **Automation:** Can be automated using .NET testing frameworks, potentially requiring setup/teardown logic for the sandbox environment.

### 2.3. End-to-End (E2E) Testing

*   **Scope:** Testing the complete workflow from an MCP client request to the MCP server response, involving the entire stack.
*   **Environment:** Requires the Aspire MCP server application to be running and configured to connect to the **Aspire Sandbox Environment**.
*   **Tools:** Primarily uses the **MCP Inspector** tool.
*   **Focus:** Simulating real-world usage. Verifying that MCP tool requests sent via the Inspector are correctly routed, processed by the appropriate handlers, interact with the sandbox API as needed, and return the expected responses according to the tool definitions. Tests cover various input parameters, edge cases, and error scenarios visible to the MCP client.
    *   **Scenarios:** Use the MCP Inspector to trigger `ListProperties`, `ListEquipment`, and `CreateUpdateProperty` tools with various valid and invalid inputs. Observe the responses and server logs. Test sequences of operations, e.g., create a property, then list properties to see if the new property appears and if caching behaves as expected.
*   **Automation:** Initially manual using the MCP Inspector. Automation could be explored in the future using scripting or specialized MCP testing tools if available.

### 2.4. Performance Testing (Caching Verification)

*   **Scope:** Evaluating the performance impact of the advanced caching strategy.
*   **Environment:** Aspire Sandbox Environment.
*   **Tools:** Load testing tools like Apache JMeter, k6, or custom scripts.
*   **Methodology:**
    *   Design test cases that simulate realistic usage patterns, including repeated requests for the same data (to test cache hits) and requests for new data (to test cache misses and population).
    *   Measure and compare response times for cached vs. non-cached requests for `ListPropertiesHandler` and `ListEquipmentHandler`.
    *   Simulate concurrent requests to evaluate cache performance and stability under load.
    *   Monitor server resource usage (CPU, memory) during load tests.
    *   Define performance metrics and thresholds (e.g., target response times for cached requests).

### 2.5. Error Cases to Validate

*   **General:**
    *   Invalid or missing required input parameters for any handler.
    *   Authentication/Authorization failures when interacting with the Aspire Sandbox API.
    *   Network errors or timeouts during API calls.
    *   Unexpected API responses or data formats.
*   **Handler Specific:**
    *   `CreateUpdatePropertyHandler`: Attempting to create a property with invalid data (e.g., missing required fields, invalid formats). Attempting to update a non-existent property.
    *   `ListPropertiesHandler`, `ListEquipmentHandler`: Handling empty responses from the API. Handling API errors during data fetching.
*   **Caching Specific:**
    *   Cache storage failures (if using an external cache).
    *   Serialization/Deserialization errors for cached data.
    *   Cache invalidation failures (e.g., cache not updating after a data modification).
    *   Handling stale data if cache expiration/invalidation mechanisms fail.

## 3. Tools and Environments

*   **Aspire Sandbox:** The designated environment for integration, performance, and E2E testing, providing a safe replica of the production Aspire Cloud API. Test data management within the sandbox is crucial (see Section 4).
*   **MCP Inspector:** The primary tool for manual E2E testing and debugging tool interactions. Allows sending specific requests and inspecting responses.
*   **.NET Testing Frameworks (xUnit/MSTest):** Used for writing and running automated unit and integration tests.
*   **Mocking Libraries (Moq/NSubstitute):** Used in unit tests to isolate components by replacing dependencies with mock objects.
*   **Load Testing Tools (JMeter, k6, etc.):** Used for performance testing of the caching strategy.

## 4. Test Data Management

*   **Requirement:** Representative and consistent test data must be available within the Aspire Sandbox (e.g., specific contacts, jobs with various statuses, time entries, inventory items, **Properties with different attributes, Equipment records**).
*   **Strategy:**
    *   Define standard datasets required for various test scenarios, including specific data for testing `ListProperties`, `ListEquipment`, and `CreateUpdateProperty` handlers.
    *   Include data sets specifically designed to test caching scenarios (e.g., data that will be frequently accessed, data that will be updated).
    *   Develop procedures (manual or scripted) for setting up, tearing down, or resetting sandbox data to ensure repeatable tests.
    *   Document the standard test data available in the sandbox.

## 5. Automation Strategy

*   **Unit Tests:** Will be fully automated using `dotnet test` and run frequently. Integration into a CI/CD pipeline is recommended if implemented.
*   **Integration Tests:** Will be automated where feasible, run as part of the build or a dedicated integration test suite. Requires managing sandbox interactions.
*   **Performance Tests:** Automation is crucial for repeatable performance measurements. Integrate load tests into a pipeline if possible.
*   **E2E Tests:** Initially manual via MCP Inspector. Automation is a future consideration, potentially requiring custom tooling or scripts.

## 6. Integration with Workflow

*   Testing is an integral part of the development process, as outlined in `wayOfWorking.md`.
*   Developers are expected to write unit tests for new code.
*   E2E testing using the MCP Inspector should be performed frequently during development iterations.
*   Meeting the testing requirements defined here is part of the `definitionOfDone.md` for each task.