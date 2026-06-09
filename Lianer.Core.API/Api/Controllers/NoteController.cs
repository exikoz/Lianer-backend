using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activities/{activityId:guid}/notes")]
[Produces("application/json")]
public class NoteController : ControllerBase
{
    private readonly INoteService _service;
    private readonly INoteQueryService _queries;

    public NoteController(
        INoteService service,
        INoteQueryService queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoteSummary>>> List(
        Guid activityId,
        int currentPage,
        int pageSize,
        CancellationToken ct)
    {
        var notes = await _queries.GetByActivityId(activityId,currentPage,pageSize, ct);
        return Ok(notes);
    }

    [HttpGet("{noteId:guid}")]
    public async Task<ActionResult<NoteSummary>> Get(
        Guid activityId,
        Guid noteId,
        CancellationToken ct)
    {
        var note = await _queries.GetById(activityId, noteId, ct);
        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteSummary>> Create(
        Guid activityId,
        CreateNoteRecord request,
        CancellationToken ct)
    {
        if (CurrentUserId == null) return Unauthorized();
        var userId = CurrentUserId.Value;
        var id = await _service.Create(activityId,userId, request, ct);

        var created = await _queries.GetById(activityId, id, ct);

        return CreatedAtAction(
            nameof(Get),
            new
            {
                version = HttpContext.GetRequestedApiVersion()?.ToString(),
                activityId,
                noteId = id
            },
            created);
    }

    [HttpPut("{noteId:guid}")]
    public async Task<ActionResult<NoteSummary>> Update(
        Guid activityId,
        Guid noteId,
        UpdateNoteRecord request,
        CancellationToken ct)
    {
        await _service.Update(activityId, noteId, request, ct);

        var updated = await _queries.GetById(activityId, noteId, ct);

        return Ok(updated);
    }

    [HttpDelete("{noteId:guid}")]
    public async Task<IActionResult> Delete(
        Guid activityId,
        Guid noteId,
        CancellationToken ct)
    {
        await _service.Delete(activityId, noteId, ct);

        return NoContent();
    }

    private Guid? CurrentUserId 
    {
        get
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claimValue, out var guid) ? guid : null;
        }
    }
}