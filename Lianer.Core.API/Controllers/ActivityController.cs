using Asp.Versioning;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for activity management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activities")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;
    private readonly IActivityQueryService _queries;
    private readonly ILogger<ActivityController> _logger;
    private readonly string BaseRoute = "/api/v1/activities";

    public ActivityController(
        IActivityService service,
        IActivityQueryService queries,
        ILogger<ActivityController> logger)
    {
        _service = service;
        _logger = logger;
        _queries = queries;
    }

    /// <summary>
    /// Gets the latest activities for a specific user.
    /// </summary>
    /// <param name="id">User id.</param>
    /// <param name="currentPage">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="ct"></param>
    /// <returns>A paginated list of activities.</returns>
    [HttpGet("user/{id:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivitySummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ActivitySummary>>> ListActivitiesByUserId(
        [FromRoute] Guid id,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken ct)
    {
        _logger.LogInformation("GET {BaseRoute}/user/{UserId} called", BaseRoute, id);

        var activities = await _queries.GetLatestActivitiesByUserId(
            id,
            currentPage,
            pageSize,
            ct);

        return Ok(activities);
    }

    /// <summary>
    /// Gets a specific activity by id.
    /// </summary>
    /// <param name="id">Activity id.</param>
    /// <param name="ct"></param>
    /// <returns>The requested activity.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivitySummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivitySummary>> GetActivityById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        _logger.LogInformation("GET {BaseRoute}/{ActivityId} called", BaseRoute, id);

        var activity = await _service.GetActivityById(id, ct);

        return Ok(activity);
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="request">Activity creation data.</param>
    /// <param name="ct"></param>
    /// <returns>The created activity.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ActivitySummary), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivitySummary>> CreateActivity(
        [FromBody] CreateActivityRecord request,
        CancellationToken ct)
    {
        _logger.LogInformation("POST {BaseRoute} called", BaseRoute);

        var id = await _service.Create(request, ct);

        var created = await _queries.GetActivitySummaryById(id, ct);

        return CreatedAtAction(
            nameof(GetActivityById),
            new
            {
                version = HttpContext.GetRequestedApiVersion()?.ToString(),
                id
            },
            created);
    }

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="request">Updated activity data.</param>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns>The updated activity.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivitySummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivitySummary>> UpdateActivity(
        [FromBody] UpdateActivityRecord request,
        Guid id,
        CancellationToken ct)
    {
        _logger.LogInformation("PUT {BaseRoute} called", BaseRoute);

        var response = await _service.Update(id, request,ct);

        var updated = await _queries.GetActivitySummaryById(response, ct);

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActivity(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        _logger.LogInformation("DELETE {BaseRoute}/{Id} called", BaseRoute, id);

        await _service.Delete(id, ct);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ActivitySummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivitySummary>>> ListLatestActivities(
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken ct)
    {
        var activities = await _queries.GetLastUpdatedActivities(currentPage, pageSize, ct);

        return Ok(activities);
    }
}