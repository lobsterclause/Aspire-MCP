using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using AspireAPI.Models; // Added using directive for Models

namespace AspireAPI;

public class AspireApiHandlers
{
    private readonly TokenService _tokenService;
    private readonly ILogger<AspireApiHandlers> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public AspireApiHandlers(TokenService tokenService, ILogger<AspireApiHandlers> logger, IHttpClientFactory httpClientFactory)
    {
        _tokenService = tokenService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CallToolResponse> HandleListBranchesAsync(
        IDictionary<string, object> arguments,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var (id, name, pageNumber, pageSize) = ExtractListBranchesArguments(arguments);

        var responseContent = await GetBranchesFromApiAsync(id, name, pageNumber, pageSize, accessToken, cancellationToken);

        return new CallToolResponse
        {
            Content = new[]
            {
                new Content
                {
                    Text = responseContent
                }
            }
        };
    }

    private (string id, string name, int pageNumber, int pageSize) ExtractListBranchesArguments(IDictionary<string, object> arguments)
    {
        string id = null;
        if (arguments.TryGetValue("id", out var idObj) && idObj != null)
        {
            id = idObj.ToString();
        }

        string name = null;
        if (arguments.TryGetValue("name", out var nameObj) && nameObj != null)
        {
            name = nameObj.ToString();
        }

        int pageNumber = 1;
        if (arguments.TryGetValue("pageNumber", out var pageNumberObj) && pageNumberObj != null)
        {
            pageNumber = Convert.ToInt32(pageNumberObj);
        }

        int pageSize = 100;
        if (arguments.TryGetValue("pageSize", out var pageSizeObj) && pageSizeObj != null)
        {
            pageSize = Convert.ToInt32(pageSizeObj);
        }

        return (id, name, pageNumber, pageSize);
    }

    private async Task<string> GetBranchesFromApiAsync(string id, string name, int pageNumber, int pageSize, string accessToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(id))
        {
            queryParams.Add($"id={Uri.EscapeDataString(id)}");
        }

        if (!string.IsNullOrEmpty(name))
        {
            queryParams.Add($"name={Uri.EscapeDataString(name)}");
        }

        var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : "";
        var url = $"https://cloud-api.youraspire.com/Branches{queryString}";

        var response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new McpServerException($"Error calling Aspire API: {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}