## Way of Working

## Development Processes
- **Task Breakdown:** Break down larger goals from `projectRoadmap.md` into smaller, manageable tasks documented in `currentTask.md`.
- **Iterative Development:** Implement features and tools incrementally, testing frequently.
- **Testing:** Prioritize frequent testing by running the application and using the MCP Inspector.
- **Documentation:** Keep `cline_docs` updated with significant changes and decisions.
- **Version Control:** Use Git for tracking changes. Commit changes after completing each task or making significant updates.

## Coding Standards and Conventions
- Follow standard C# coding conventions and best practices.
- Ensure code is readable, maintainable, and well-commented where necessary.
- Strictly adhere to the following limits:
  - Maximum of 200 lines per file
  - Maximum of 50 lines per function or method
- Use class inheritance and composition patterns to reduce code duplication
- Implement specialized classes with single responsibilities
- Utilize dependency injection for better testability and loose coupling
- Apply consistent error handling and logging patterns across all components

## Environment Setup and Configuration
- Rely on environment variables for configuration, especially for sensitive API credentials.
- Use .NET Aspire's configuration management capabilities.

## Performance and Optimization
- Consider performance implications when implementing tools, especially for data retrieval and processing.
- Implement caching as a future optimization step.

## Debugging and Troubleshooting
- Utilize .NET Aspire's diagnostics and logging features.
- Use the MCP Inspector to test tool inputs and outputs.
- Implement consistent error handling across all handlers:
  - Catch exceptions at appropriate boundaries
  - Log detailed error information including stack traces
  - Return descriptive error messages to clients

## Regular Maintenance
- Periodically review and refactor code for technical debt.
- Keep dependencies updated.
- Enforce code structure guidelines during code reviews.
- Regularly validate that files stay within the 200-line limit.
- Consider extracting common patterns into reusable components when possible.