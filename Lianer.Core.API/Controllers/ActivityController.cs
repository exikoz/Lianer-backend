using Asp.Versioning;
using Azure;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.DTOs.User;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for user management (RESTful resource: users)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activities")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;
    private readonly ILogger<ActivityController> _logger;
    private readonly string BaseRoute = "/api/v1/activities"; 
    public ActivityController(IActivityService service, ILogger<ActivityController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// For fetching latest activities by user id
    /// </summary>
    /// <returns>Pages IReadOnlyList of activities</returns>
    [HttpGet("user/{id:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivitySummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivitySummary>>> ListActivitiesByUserId(
        [FromRoute] Guid id, [FromQuery] int currentPage, [FromQuery] int pageSize, CancellationToken ct)
    {
        _logger.LogInformation("GET {BaseRoute} called", BaseRoute);

        var activities = await _service.GetLatestActivitiesByUserId(id, currentPage, pageSize, ct);
        return Ok(activities);
    }

    /// <summary>
    /// For fetching latest activities by user id
    /// </summary>
    /// <returns>Pages IReadOnlyList of activities</returns>
    [HttpGet("activity/{id:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivitySummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivitySummary>>> GetActivityById(
        [FromRoute] Guid id, [FromQuery] int currentPage, [FromQuery] int pageSize, CancellationToken ct)
    {
        _logger.LogInformation("GET {BaseRoute} called", BaseRoute);

        var activities = await _service.GetActivityById(id, ct);
        return Ok(activities);
    }

    /// <summary>
    /// Creates a new user account (registration)
    /// </summary>
    /// <param name="request">User data (name, email, password)</param>
    /// <returns>Created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input or email already registered</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponseDto>> CreateActivity([FromBody] CreateActivityRecord request, CancellationToken ct)
    {
        _logger.LogInformation("POST /api/v1/users called");

        var response = await _service.Create(request,ct);

        return CreatedAtAction(
            nameof(GetActivityById),
            new { 
                version = "1.0",
                id = response 
                },
            
            response
        );
    }

 
}
