# Aspire MCP Server: User Experience Journey

## Overview

This document outlines the user experience journey for interacting with the Aspire landscape management system through an LLM (Large Language Model) interface. The MCP (Model Context Protocol) server acts as a bridge between the LLM and Aspire's API, enabling natural language interactions with landscape management data.

## User Personas

### Primary Users
1. **Landscape Business Owners**: Need high-level business insights and reporting
2. **Project Managers**: Need to check schedules, properties, and team assignments
3. **Administrative Staff**: Need to manage contacts, invoices, and payments
4. **Field Workers**: Need to update job status and view assigned work

## Setup Journey

### Initial Configuration
1. **Installation**: User installs the Aspire MCP server locally or configures it in their cloud environment
2. **API Authentication**: User provides Aspire API credentials (API key, account info)
3. **Connection Verification**: System validates connection to Aspire API
4. **LLM Integration**: User connects their preferred LLM interface to the MCP server

## Key User Journeys

### 1. Property Management Workflow

**User Goal**: Retrieve and manage property information.

**Journey Steps**:
1. **User Query**: "Show me all active commercial properties in Austin, TX"
2. **LLM Processing**: The LLM recognizes this as a property retrieval request
3. **Tool Selection**: LLM selects the `list_properties` MCP tool
4. **Tool Execution**: MCP server makes API call to Aspire with appropriate filters
5. **Data Presentation**: Results are formatted and presented to the user
6. **Follow-up Actions**: User can request additional details, updates, or schedules for these properties

### 2. Schedule Management Workflow

**User Goal**: Check and manage job scheduling information.

**Journey Steps**:
1. **User Query**: "What jobs are scheduled for next Tuesday?"
2. **LLM Processing**: The LLM processes the date and request type
3. **Tool Selection**: LLM selects the `get_schedule_board` MCP tool
4. **Parameter Preparation**: Date is formatted appropriately for the API
5. **Tool Execution**: MCP server requests schedule data from Aspire API
6. **Data Presentation**: Schedule information is displayed in a structured format
7. **Follow-up Options**: User can ask to reschedule jobs, view details, or filter further

### 3. Financial Overview Workflow

**User Goal**: Get financial insights about the business.

**Journey Steps**:
1. **User Query**: "Generate a report on unpaid invoices over 30 days"
2. **LLM Processing**: The LLM recognizes this as a financial reporting request
3. **Tool Selection**: LLM selects the `generate_report` MCP tool
4. **Parameter Configuration**: Report parameters are set for invoice age > 30 days, status = unpaid
5. **Tool Execution**: MCP server requests financial data from Aspire API
6. **Report Generation**: Data is aggregated into a meaningful report
7. **Data Presentation**: Report is presented to the user with key insights highlighted
8. **Follow-up Options**: User can drill down into specific invoices or payment processing

### 4. Contact Management Workflow

**User Goal**: Find and manage customer contact information.

**Journey Steps**:
1. **User Query**: "Find contact information for Riverdale Apartments"
2. **LLM Processing**: The LLM identifies this as a contact retrieval request
3. **Tool Selection**: LLM selects the `list_contacts` MCP tool
4. **Tool Execution**: MCP server searches contacts via the Aspire API
5. **Data Presentation**: Contact details are presented to the user
6. **Follow-up Options**: User can request to update information, view associated properties, or check payment history

## User Experience Benefits

1. **Natural Language Interface**: Users interact with complex business data using everyday language
2. **Context-Aware Conversations**: The LLM maintains context during multi-turn conversations about business data
3. **Simplified Workflow**: Users bypass navigating complex UI menus with natural queries
4. **Intelligent Summarization**: The LLM can analyze and summarize large datasets into actionable insights
5. **Proactive Suggestions**: Based on data patterns, the system can suggest follow-up actions

## Technical Journey (Behind the Scenes)

1. **Authentication Flow**: 
   - MCP server stores and manages OAuth tokens for Aspire API
   - Handles token refresh and authentication errors transparently

2. **Request Processing**:
   - User's natural language request is processed by the LLM
   - LLM identifies intent and selects appropriate MCP tool
   - Tool parameters are extracted from the natural language query
   - MCP server converts tool parameters to Aspire API parameters
   - API request is made and response is received
   - Response is transformed into LLM-friendly format
   - LLM presents information to the user in natural language

3. **Error Handling**:
   - Network issues are properly communicated
   - API rate limits are managed
   - Credential issues prompt re-authentication
   - Malformed requests are reported with helpful context

## Future Enhancements

1. **Proactive Notifications**: Alert users about upcoming scheduling conflicts or payment deadlines
2. **Multi-modal Interactions**: Support for image-based interactions (e.g., uploading site photos)
3. **Offline Support**: Basic functionality when internet connection is unavailable
4. **User Preference Learning**: System adapts to user's reporting and query preferences over time

## Conclusion

The Aspire MCP server creates a seamless bridge between natural language interaction and the structured data of the Aspire landscape management system. By enabling users to query business data, generate reports, and manage operations through conversational language, it removes technical barriers and allows focus on the actual business insights and decisions.