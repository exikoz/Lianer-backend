using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

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
        CancellationToken ct)
    {
        var notes = await _queries.GetByActivityId(activityId, ct);
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
        var id = await _service.Create(activityId, request, ct);

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
}