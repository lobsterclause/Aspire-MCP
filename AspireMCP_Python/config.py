import os

# Aspire API Configuration
ASPIRE_API_BASE_URL = os.environ.get("ASPIRE_API_BASE_URL", "https://cloudsandbox-api.youraspire.com")
ASPIRE_CLIENT_ID = os.environ.get("ASPIRE_CLIENT_ID", "YOUR_CLIENT_ID") # Placeholder
ASPIRE_CLIENT_SECRET = os.environ.get("ASPIRE_CLIENT_SECRET", None) # No default, will be None if not set
ASPIRE_SUBSCRIPTION_KEY = os.environ.get("ASPIRE_SUBSCRIPTION_KEY", "YOUR_SUBSCRIPTION_KEY") # Placeholder

# MCP Server Configuration
MCP_MODE = os.environ.get("MCP_MODE", "stdio") # stdio or sse (future)