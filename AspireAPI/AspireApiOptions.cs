using System;
using System.Collections.Generic;

namespace AspireAPI
{
    /// <summary>
    /// Configuration options for the AspireAPI service
    /// </summary>
    public class AspireApiOptions
    {
        /// <summary>
        /// Gets or sets the base URL for the Aspire Cloud API. Default is the
        /// production-write-guarded sandbox host so a fresh clone with no config
        /// cannot accidentally hit the production tenant. Override in
        /// appsettings.json (or AspireApi__BaseUrl env var) to point at production.
        /// </summary>
        public string BaseUrl { get; set; } = "https://cloudsandbox-api.youraspire.com";
        
        /// <summary>
        /// Gets or sets the API key for authenticating with the Aspire Cloud API
        /// </summary>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Gets or sets the client ID for authenticating with the OAuth server
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the client secret for authenticating with the OAuth server
        /// </summary>
        public string ClientSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the OAuth server URL
        /// </summary>
        public string OAuthServerUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the authentication configuration
        /// </summary>
        public AuthConfig Auth { get; set; } = new AuthConfig();
        
        /// <summary>
        /// Gets or sets the timeout configuration
        /// </summary>
        public TimeoutConfig Timeouts { get; set; } = new TimeoutConfig();
        
        /// <summary>
        /// Gets or sets the retry configuration
        /// </summary>
        public RetryConfig Retries { get; set; } = new RetryConfig();
    }
    
    /// <summary>
    /// Configuration for request timeouts
    /// </summary>
    public class TimeoutConfig
    {
        /// <summary>
        /// Gets or sets the request timeout in seconds
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 10;
    }
    
    /// <summary>
    /// Configuration for request retries
    /// </summary>
    public class RetryConfig
    {
        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetries { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the delay between retries in seconds
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 2;
    }
    
    /// <summary>
    /// Configuration for authentication
    /// </summary>
    public class AuthConfig
    {
        /// <summary>
        /// Gets or sets whether to use token-based authentication
        /// </summary>
        public bool UseTokenAuth { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the username for token-based authentication
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password for token-based authentication
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the company key for token-based authentication
        /// </summary>
        public string CompanyKey { get; set; }
        
        /// <summary>
        /// Gets or sets the number of seconds before token expiration to trigger a refresh
        /// </summary>
        public int RefreshTokenBeforeExpirationSeconds { get; set; } = 300;
    }
}