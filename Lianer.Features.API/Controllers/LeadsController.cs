using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Asp.Versioning;
using Lianer.Features.API.Clients;
using Lianer.Features.API.Data;
using Lianer.Features.API.DTOs;
using Lianer.Features.API.DTOs.Integration;
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
public class LeadsController(
    IHunterService hunterService, 
    CoreApiClient coreApiClient, 
    FeaturesDbContext dbContext, 
    IMemoryCache cache,
    ILogger<LeadsController> logger) : ControllerBase
{
    private readonly ILogger<LeadsController> _logger = logger;
    private const string LeadsCacheKeyPrefix = "enriched_leads_list";
    private static CancellationTokenSource _cacheResetToken = new();

    /// <summary>
    /// Invalidates all cached lead queries by cancelling the shared token.
    /// </summary>
    private static void InvalidateLeadsCache()
    {
        var oldToken = Interlocked.Exchange(ref _cacheResetToken, new CancellationTokenSource());
        oldToken.Cancel();
        oldToken.Dispose();
    }

    /// <summary>
    /// Gets all leads currently in the features database, enriched with assigned user names from Core API.
    /// Supports pagination, sorting and search filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="search">Optional search term to filter by name, email or organization</param>
    /// <param name="sortBy">Sort field: name, email, organization, created (default: created)</param>
    /// <param name="sortOrder">Sort direction: asc or desc (default: desc)</param>
    /// <returns>A paginated list of enriched leads</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "created",
        [FromQuery] string sortOrder = "desc")
    {
        _logger.LogInformation("GET /api/v1/leads called (page={Page}, pageSize={PageSize}, search={Search}, sortBy={SortBy})", 
            page, pageSize, search, sortBy);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // Build cache key based on query parameters
        var cacheKey = $"{LeadsCacheKeyPrefix}_p{page}_s{pageSize}_{search}_{sortBy}_{sortOrder}";

        if (cache.TryGetValue(cacheKey, out object? cachedResult))
        {
            _logger.LogInformation("Returning leads from cache.");
            return Ok(cachedResult);
        }

        _logger.LogInformation("Cache miss. Fetching fresh data and enriching.");

        // 1. Build query with filtering
        IQueryable<Contact> query = dbContext.Contacts;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.Organization.ToLower().Contains(term));
        }

        // 2. Sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortOrder == "asc" 
                ? query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName) 
                : query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName),
            "email" => sortOrder == "asc" ? query.OrderBy(c => c.Email) : query.OrderByDescending(c => c.Email),
            "organization" => sortOrder == "asc" ? query.OrderBy(c => c.Organization) : query.OrderByDescending(c => c.Organization),
            _ => sortOrder == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt)
        };

        // 3. Get total count for pagination metadata
        var totalCount = await query.CountAsync();

        // 4. Apply pagination
        var leads = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 5. Fetch all users from Core API for mapping
        IEnumerable<CoreUserSummaryDto> users = [];
        try
        {
            users = await coreApiClient.GetUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch users from Core API for enrichment. Listing leads without names.");
        }

        // 6. Create a lookup dictionary for performance
        var userMap = users.ToDictionary(u => u.UserId, u => u.FullName);

        // 7. Map to enriched result
        var enrichedLeads = leads.Select(l => new
        {
            l.Id,
            l.FirstName,
            l.LastName,
            l.Email,
            l.Organization,
            l.Position,
            l.Source,
            AssignedToId = l.AssignedTo,
            AssignedToName = l.AssignedTo.HasValue && userMap.TryGetValue(l.AssignedTo.Value, out var name) 
                ? name 
                : (l.AssignedTo.HasValue ? "User not found" : "Unassigned")
        }).ToList();

        var result = new
        {
            Data = enrichedLeads,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        // 8. Store in cache with eviction token
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))
            .AddExpirationToken(new CancellationChangeToken(_cacheResetToken.Token));

        cache.Set(cacheKey, result, cacheOptions);

        return Ok(result);
    }

    /// <summary>
    /// Temporary endpoint to prepare test data for K-126 verification.
    /// Dynamically links the latest lead with a live user from Core API.
    /// </summary>
    [HttpPost("prepare-test")]
    [Authorize]
    public async Task<IActionResult> PrepareTest()
    {
        _logger.LogInformation("POST /api/v1/leads/prepare-test called (Bulk Mode)");

        // 1. Fetch all leads
        var allLeads = await dbContext.Contacts.ToListAsync();

        if (allLeads == null || !allLeads.Any())
        {
            return NotFound("No leads found in the database. Please import some first!");
        }

        // 2. Fetch a live user from Core API
        try
        {
            var users = await coreApiClient.GetUsersAsync();
            var targetUser = users.FirstOrDefault();

            if (targetUser == null)
            {
                return BadRequest("No users found in Core API. Please create a user first!");
            }

            // 3. Link ALL leads to this user
            foreach (var lead in allLeads)
            {
                lead.AssignedTo = targetUser.UserId;
            }
            
            await dbContext.SaveChangesAsync();

            // Invalidate cache
            InvalidateLeadsCache();

            _logger.LogInformation("Linked {Count} leads to user {UserId} and invalidated cache", allLeads.Count, targetUser.UserId);

            return Ok(new
            {
                Message = $"All {allLeads.Count} leads have been successfully assigned to [{targetUser.FullName}]",
                AssignedToId = targetUser.UserId,
                AssignedToName = targetUser.FullName,
                AffectedLeads = allLeads.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk test preparation");
            return StatusCode(StatusCodes.Status500InternalServerError, "Could not communicate with Core API to fetch users.");
        }
    }

    /// <summary>
    /// Request model for assigning a lead to a user.
    /// </summary>
    public class AssignLeadRequest
    {
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// Manually assigns a specific lead to a user.
    /// </summary>
    /// <param name="leadId">The ID of the lead to assign</param>
    /// <param name="request">The assignment request containing the target User ID</param>
    [HttpPatch("{leadId}/assign")]
    [Authorize]
    public async Task<IActionResult> AssignLead([FromRoute] Guid leadId, [FromBody] AssignLeadRequest request)
    {
        _logger.LogInformation("PATCH /api/v1/leads/{LeadId}/assign called with User {UserId}", leadId, request.UserId);

        // 1. Find the lead
        var lead = await dbContext.Contacts.FindAsync(leadId);
        if (lead == null) return NotFound("Lead not found.");

        // 2. Optional: Verify user exists in Core API (Integration Check)
        try
        {
            var user = await coreApiClient.GetUserSummaryAsync(request.UserId);
            if (user == null) return BadRequest("The specified User ID was not found in Core API.");
            
            // 3. Update and Save
            lead.AssignedTo = request.UserId;
            await dbContext.SaveChangesAsync();

            // Invalidate cache
            InvalidateLeadsCache();

            return Ok(new { 
                Message = $"Lead [{lead.FirstName} {lead.LastName}] has been manually assigned to [{user.FullName}] (Cache invalidated)",
                LeadId = lead.Id,
                AssignedToName = user.FullName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying user during manual assignment");
            return StatusCode(StatusCodes.Status500InternalServerError, "Could not verify user with Core API.");
        }
    }

    /// <summary>
    /// Gets detailed information about a lead, including enriched data from the Core API
    /// </summary>
    /// <param name="id">The unique identifier of the lead</param>
    /// <returns>Enriched lead details</returns>
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetLeadDetails(Guid id)
    {
        _logger.LogInformation("GET /api/v1/leads/{Id}/details called", id);

        var lead = await dbContext.Contacts.FindAsync(id);
        if (lead == null)
        {
            return NotFound();
        }

        string? assignedToName = null;
        if (lead.AssignedTo.HasValue)
        {
            try
            {
                var userSummary = await coreApiClient.GetUserSummaryAsync(lead.AssignedTo.Value);
                assignedToName = userSummary?.FullName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch user summary for assigned user {UserId}", lead.AssignedTo.Value);
                // We continue even if the external call fails, but without the name
            }
        }

        return Ok(new
        {
            lead.Id,
            lead.FirstName,
            lead.LastName,
            lead.Email,
            lead.Organization,
            lead.Position,
            AssignedToId = lead.AssignedTo,
            AssignedToName = assignedToName ?? "Unassigned or user not found"
        });
    }

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
    [Authorize]
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
                
                // Invalidate cache
                InvalidateLeadsCache();
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
