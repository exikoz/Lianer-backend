using System.Net.Http.Json;
using Lianer.Features.API.DTOs;

namespace Lianer.Features.API.Clients;

/// <summary>
/// Typed Client for Hunter.io API according to Epic 6.
/// Includes security via API Key and error handling via EnsureSuccessStatusCode.
/// </summary>
/// <summary>
/// Typed Client for Hunter.io API according to Epic 6.
/// Handles raw HTTP communication and authentication.
/// </summary>
public class HunterClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<HunterClient> _logger;

    public HunterClient(HttpClient httpClient, IConfiguration configuration, ILogger<HunterClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Hunter:ApiKey"] 
            ?? throw new InvalidOperationException("Hunter API Key is missing. Ensure it is set in User Secrets or Key Vault.");
    }

    /// <summary>
    /// Searches for emails associated with a specific domain
    /// </summary>
    /// <param name="domain">The domain name (e.g. stripe.com)</param>
    /// <returns>Hunter.io response DTO</returns>
    public async Task<HunterResponseDto?> SearchDomainAsync(string domain)
    {
        _logger.LogInformation("Calling Hunter.io Domain Search for: {Domain}", domain);

        // Fulfills K-132: Using X-API-KEY header instead of query string
        using var request = new HttpRequestMessage(HttpMethod.Get, $"v2/domain-search?domain={domain}");
        request.Headers.Add("X-API-KEY", _apiKey);
        
        var response = await _httpClient.SendAsync(request);
        
        // Fulfills Epic 6 requirement: "hantera nätverksfel med EnsureSuccessStatusCode()"
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<HunterResponseDto>();
    }

    /// <summary>
    /// Finds a specific person's email address using their name and domain/company
    /// </summary>
    /// <param name="domain">Company domain</param>
    /// <param name="firstName">Person's first name</param>
    /// <param name="lastName">Person's last name</param>
    /// <returns>Hunter.io Email Finder response DTO</returns>
    public async Task<HunterEmailFinderResponseDto?> FindEmailAsync(string domain, string firstName, string lastName)
    {
        _logger.LogInformation("Calling Hunter.io Email Finder for: {FirstName} {LastName} at {Domain}", firstName, lastName, domain);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"v2/email-finder?domain={domain}&first_name={Uri.EscapeDataString(firstName)}&last_name={Uri.EscapeDataString(lastName)}");
        request.Headers.Add("X-API-KEY", _apiKey);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<HunterEmailFinderResponseDto>();
    }
}
