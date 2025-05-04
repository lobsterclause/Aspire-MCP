using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AspireAPI.Models
{
    /// <summary>
    /// Standard response format for tool operations
    /// </summary>
    public class ToolResponse
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The result of the operation
        /// </summary>
        public Dictionary<string, object> Result { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Error information if Success is false
        /// </summary>
        public ToolError Error { get; set; }
    }

    /// <summary>
    /// Error information for tool operations
    /// </summary>
    public class ToolError
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Additional error details
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Standard API result format
    /// </summary>
    /// <typeparam name="T">Type of data in the result</typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// Whether the API call was successful
        /// </summary>
        public bool Success { get; set; } = true;
    
        /// <summary>
        /// Data returned by the API call
        /// </summary>
        public T Data { get; set; }
    
        /// <summary>
        /// Error message if Success is false
        /// </summary>
        public string ErrorMessage { get; set; }
    
        /// <summary>
        /// Error code if Success is false
        /// </summary>
        public string ErrorCode { get; set; }
        
        /// <summary>
        /// Total count of items (for pagination)
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Indicates if the result represents an error
        /// </summary>
        public bool IsError { get; set; }
        
        
        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <returns>An error result</returns>
        public static ApiResult<T> Error(string errorMessage, string errorCode = "ERROR")
        {
            return new ApiResult<T> { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode, IsError = true };
        }
    }

    /// <summary>
    /// Non-generic API result for simpler operations
    /// </summary>
    public class ApiResult : ApiResult<object>
    {
        /// <summary>
        /// Create a successful result
        /// </summary>
        /// <returns>A success result</returns>
        public static ApiResult Success() => new ApiResult { Success = true };

        /// <summary>
        /// Create a successful result with data
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <returns>A success result with data</returns>
        public static ApiResult<object> Success(object data) => new ApiResult<object> { Success = true, Data = data };

        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <returns>An error result</returns>
        public static ApiResult Error(string errorMessage, string errorCode = "ERROR")
        {
            return new ApiResult { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode, IsError = true };
        }
    }
}