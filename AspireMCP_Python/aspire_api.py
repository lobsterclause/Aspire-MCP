import requests
import os
import logging # Import logging module
from datetime import datetime, timedelta
from config import ASPIRE_API_BASE_URL, ASPIRE_CLIENT_ID, ASPIRE_CLIENT_SECRET, ASPIRE_SUBSCRIPTION_KEY

# Configure basic logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class AspireAPI:
    def __init__(self):
        self.base_url = ASPIRE_API_BASE_URL
        self.client_id = ASPIRE_CLIENT_ID
        self.client_secret = ASPIRE_CLIENT_SECRET
        self.subscription_key = ASPIRE_SUBSCRIPTION_KEY
        self._access_token = None
        self._token_expiry = None

    async def _get_access_token(self):
        if self._access_token and self._token_expiry and datetime.now() < self._token_expiry:
            return self._access_token

        logging.info("Attempting to authenticate with Aspire API...") # Added logging
        url = f"{self.base_url}/Authorization"
        headers = {
            "Ocp-Apim-Subscription-Key": self.subscription_key,
            "Content-Type": "application/json"
        }
        payload = {
            "ClientId": self.client_id,
            "Secret": self.client_secret
        }

        try:
            # Use requests for simplicity for now, can switch to aiohttp later if needed for async
            response = requests.post(url, headers=headers, json=payload)
            response.raise_for_status() # Raise an exception for bad status codes
            data = response.json()
            self._access_token = data["accessToken"]
            # Assuming token expires in 3600 seconds (1 hour), adjust if API docs specify otherwise
            self._token_expiry = datetime.now() + timedelta(seconds=data.get("expiresIn", 3600))
            logging.info("Successfully obtained Aspire API access token.") # Added logging
            return self._access_token
        except requests.exceptions.RequestException as e:
            # Added detailed error logging
            if hasattr(e, 'response') and e.response is not None:
                 logging.error(f"Aspire API authentication failed. Status: {e.response.status_code}, Response: {e.response.text}")
            else:
                 logging.error(f"Aspire API authentication failed. Error: {e}")
            # In a real application, you might want more sophisticated error handling
            return None

    async def _refresh_token(self):
        # This is a placeholder for refresh token logic if the API supports it
        # The provided overview doesn't detail refresh tokens, so we'll re-authenticate for now
        logging.info("Attempting to refresh token (re-authenticating)...") # Added logging
        return await self._get_access_token()

    async def make_request(self, method, endpoint, **kwargs):
        token = await self._get_access_token()
        if not token:
            logging.error("Could not obtain access token, request failed.") # Added logging
            return None

        url = f"{self.base_url}{endpoint}"
        headers = {
            "Ocp-Apim-Subscription-Key": self.subscription_key,
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }
        headers.update(kwargs.pop("headers", {}))

        try:
            response = requests.request(method, url, headers=headers, **kwargs)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            logging.error(f"API request failed: {e}") # Added logging
            # Handle token expiration specifically if needed
            if hasattr(e, 'response') and e.response is not None and e.response.status_code == 401: # Example status code for unauthorized/expired token
                 logging.warning("Token might be expired, attempting to refresh and retry...") # Added logging
                 token = await self._refresh_token()
                 if token:
                     headers["Authorization"] = f"Bearer {token}"
                     response = requests.request(method, url, headers=headers, **kwargs)
                     response.raise_for_status()
                     return response.json()
            return None

    async def get_api_version(self):
        """Fetches the Aspire API version."""
        logging.info("Attempting to fetch API version...") # Added logging
        endpoint = "/Version/GetApiVersion"
        try:
            version_data = await self.make_request("GET", endpoint)
            logging.info("API version fetched successfully.") # Added logging
            return version_data
        except Exception as e:
            logging.error(f"Error fetching API version: {e}") # Added logging
            return None

    async def get_branches(self):
        """Fetches the list of branches from the Aspire API."""
        logging.info("Attempting to fetch branches...") # Added logging
        endpoint = "/Branches"
        try:
            branches_data = await self.make_request("GET", endpoint)
            logging.info("Branches fetched successfully.") # Added logging
            return branches_data
        except Exception as e:
            logging.error(f"Error fetching branches: {e}") # Added logging
            return None
async def get_divisions(self):
        """Fetches the list of divisions from the Aspire API."""
        logging.info("Attempting to fetch divisions...") # Added logging
        endpoint = "/Divisions"
        try:
            divisions_data = await self.make_request("GET", endpoint)
            logging.info("Divisions fetched successfully.") # Added logging
            return divisions_data
        except Exception as e:
            logging.error(f"Error fetching divisions: {e}") # Added logging
            return None

async def get_contacts(self, filter_query=None):
        """
        Fetches the list of contacts from the Aspire API.
        
        Args:
            filter_query (str, optional): OData filter string (e.g., "BranchName eq 'California'")
        """
        logging.info("Attempting to fetch contacts...") # Added logging
        endpoint = "/Contacts"
        
        params = {}
        if filter_query:
            logging.info(f"Applying filter: {filter_query}")
            params['$filter'] = filter_query
            
        try:
            contacts_data = await self.make_request("GET", endpoint, params=params)
            logging.info("Contacts fetched successfully.") # Added logging
            return contacts_data
        except Exception as e:
            logging.error(f"Error fetching contacts: {e}") # Added logging
            return None

async def get_properties(self):
        """Fetches the list of properties from the Aspire API."""
        logging.info("Attempting to fetch properties...") # Added logging
        endpoint = "/Properties"
        try:
            properties_data = await self.make_request("GET", endpoint)
            logging.info("Properties fetched successfully.") # Added logging
            return properties_data
        except Exception as e:
            logging.error(f"Error fetching properties: {e}") # Added logging
            return None

# Example usage (for testing purposes)
async def test_api_connection():
    api = AspireAPI()
    # This will attempt to get a token using the placeholder credentials
    token = await api._get_access_token()
    if token:
        logging.info(f"Successfully obtained token: {token[:10]}...") # Added logging
    else:
        logging.error("Failed to obtain token.") # Added logging

if __name__ == "__main__":
    import asyncio
    asyncio.run(test_api_connection())