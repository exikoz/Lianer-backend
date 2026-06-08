using Lianer.Features.API.Clients;
using Lianer.Features.API.DTOs;

namespace Lianer.Features.API.Services;

/// <summary>
/// Interface for Hunter.io domain enrichment service
/// </summary>
public interface IHunterService
{
    /// <summary>
    /// Retrieves organizational and email data for a domain
    /// </summary>
    Task<HunterDataDto?> EnrichDomainAsync(string domain);

    /// <summary>
    /// Finds a specific person's email and contact info
    /// </summary>
    Task<HunterEmailFinderDataDto?> FindContactEmailAsync(string domain, string firstName, string lastName);
}

/// <summary>
/// Service implementation for Hunter.io enrichment logic
/// </summary>
public class HunterService(HunterClient hunterClient, ILogger<HunterService> logger) : IHunterService
{
    private readonly ILogger<HunterService> _logger = logger;

    /// <summary>
    /// Enriches domain information by calling the Hunter.io typed client
    /// </summary>
    public async Task<HunterDataDto?> EnrichDomainAsync(string domain)
    {
        _logger.LogInformation("Enriching lead data for domain: {Domain}", domain);

        try
        {
            var response = await hunterClient.SearchDomainAsync(domain);
            return response?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to enrich domain {Domain} via Hunter.io", domain);
            throw;
        }
    }

    /// <summary>
    /// Finds a specific person's email and contact info
    /// </summary>
    public async Task<HunterEmailFinderDataDto?> FindContactEmailAsync(string domain, string firstName, string lastName)
    {
        _logger.LogInformation("Finding email for {FirstName} {LastName} at {Domain}", firstName, lastName, domain);

        try
        {
            var response = await hunterClient.FindEmailAsync(domain, firstName, lastName);
            return response?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to find email for {FirstName} {LastName} at {Domain}", firstName, lastName, domain);
            throw;
        }
    }
}
