using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AspireAPI
{
    // Local definition of missing types to support ModelContextProtocol
    public class ListToolsRequest { }
    
    public class CallToolRequest
    {
        public CallToolParams? Params { get; set; }
    }
    
    public class CallToolParams
    {
        public string? Name { get; set; }
        public object? Arguments { get; set; }
    }
    
    public class Error
    {
        public string? Message { get; set; }
    }
    
    // Additional required types
    public class Tool
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? InputSchema { get; set; }
    }
    
    public class ListToolsResult
    {
        public List<Tool>? Tools { get; set; } = new List<Tool>();
    }
    
    public class CallToolResponse
    {
        public object? Content { get; set; }
        public Error? Error { get; set; }
        
        public CallToolResponse WithContent(object content)
        {
            Content = content;
            return this;
        }
        
        public CallToolResponse WithError(string message)
        {
            Error = new Error { Message = message };
            return this;
        }
        
        // Simple ToolResponse class for internal communication
        public class ToolResponse
        {
            public bool Success { get; set; } = false;
            public object? Result { get; set; }
            public string? Error { get; set; }
        }
    }
    
    /// <summary>
    /// Standard interface for tool definitions
    /// </summary>
    public interface IToolDefinition
    {
        /// <summary>
        /// Gets the name of the tool
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the description of the tool
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the schema for the tool's input parameters
        /// </summary>
        /// <returns>JSON schema object</returns>
        Task<NJsonSchema.JsonSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Exception thrown during MCP server operations.
    /// </summary>
    public class McpServerException : Exception
    {
        public McpServerException() : base() { }
        
        public McpServerException(string message) : base(message) { }
        
        public McpServerException(string message, Exception innerException)
            : base(message, innerException) { }
            
        public string ErrorType { get; set; } = "ServerError";
    }
}