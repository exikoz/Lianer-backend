using Asp.Versioning;
using Lianer.Features.API.Data;
using Lianer.Features.API.DTOs;
using Lianer.Features.API.Models;
using Lianer.Features.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lianer.Features.API.Controllers;

/// <summary>
/// Controller for managing and enriching leads through external integrations
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ApiController]
[Authorize]
public class LeadsController(IHunterService hunterService, FeaturesDbContext dbContext, ILogger<LeadsController> logger) : ControllerBase
{
    private readonly ILogger<LeadsController> _logger = logger;

    /// <summary>
    /// Enriches a domain with lead information via Hunter.io
    /// </summary>
    /// <param name="domain">The domain to search (e.g., stripe.com)</param>
    /// <returns>Lead data including organization and email list</returns>
    /// <response code="200">Enrichment data retrieved successfully</response>
    /// <response code="404">No data found for the domain</response>
    /// <response code="500">External service error</response>
    [HttpGet("enrich/{domain}")]
    [ProducesResponseType(typeof(HunterDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HunterDataDto>> EnrichLead(string domain)
    {
        _logger.LogInformation("GET /api/v1/leads/enrich/{Domain} called", domain);

        // Validation - ensure domain is provided
        if (string.IsNullOrWhiteSpace(domain))
        {
            return BadRequest("Domain name is required.");
        }

        try
        {
            var data = await hunterService.EnrichDomainAsync(domain);
            
            if (data == null)
            {
                _logger.LogWarning("No data returned for domain: {Domain}", domain);
                return NotFound($"No lead information found for domain: {domain}");
            }

            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controller error during enrichment for domain: {Domain}", domain);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while fetching data from external provider.");
        }
    }

    /// <summary>
    /// Fetches all contacts for a domain from Hunter.io and imports them into the local database.
    /// </summary>
    /// <param name="domain">The domain to scan (e.g. stripe.com)</param>
    /// <returns>A summary of the import operation</returns>
    /// <response code="200">Successfully completed the import process</response>
    /// <response code="404">No data found for the domain</response>
    [HttpPost("import/{domain}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkImport(string domain)
    {
        _logger.LogInformation("POST /api/v1/leads/import/{Domain} called", domain);

        if (string.IsNullOrWhiteSpace(domain))
        {
            return BadRequest("Domain is required.");
        }

        try
        {
            // 1. Fetch data from Hunter.io
            var hunterData = await hunterService.EnrichDomainAsync(domain);

            if (hunterData == null || hunterData.Emails == null || hunterData.Emails.Count == 0)
            {
                return NotFound($"Could not import leads. No emails found for {domain}.");
            }

            // 2. Map payload to internal Contact entities
            var contactsToImport = hunterData.Emails.Select(e => new Contact
            {
                FirstName = e.FirstName ?? string.Empty,
                LastName = e.LastName ?? string.Empty,
                Email = e.Value ?? string.Empty,
                Position = e.Position ?? string.Empty,
                Organization = hunterData.Organization ?? domain,
                Source = "Hunter.io"
            }).Where(c => !string.IsNullOrEmpty(c.Email)).ToList();

            // 3. Filter out existing emails to avoid duplicates
            var newEmails = contactsToImport.Select(c => c.Email).ToList();
            var existingEmails = await dbContext.Contacts
                .Where(c => newEmails.Contains(c.Email))
                .Select(c => c.Email)
                .ToListAsync();

            var newContacts = contactsToImport
                .Where(c => !existingEmails.Contains(c.Email, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // 4. Save to Database
            if (newContacts.Any())
            {
                await dbContext.Contacts.AddRangeAsync(newContacts);
                await dbContext.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = $"Import completed for {domain}",
                TotalFound = contactsToImport.Count,
                Imported = newContacts.Count,
                DuplicatesSkipped = existingEmails.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controller error during bulk import for {Domain}", domain);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred during the bulk import process.");
        }
    }
}
