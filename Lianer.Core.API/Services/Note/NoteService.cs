using Lianer.Core.API.Common;

public sealed class NoteService(INoteRepository repo) : INoteService
{
    private readonly INoteRepository _repo = repo;

    public async Task<Guid> Create(
        Guid activityId,
        CreateNoteRecord request,
        CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(activityId);
        Guard.Against.Null(request);
        Guard.Against.NullOrWhiteSpace(request.Title);
        Guard.Against.NullOrWhiteSpace(request.Content);

        var note = new Note
        {
            Id = Guid.NewGuid(),
            ActivityId = activityId,
            Title = request.Title,
            Content = request.Content,
            CreatedBy = request.CreatedBy
        };

        var created = await _repo.Create(note, ct);

        return created.Id;
    }

    public async Task<Guid> Update(
        Guid activityId,
        Guid noteId,
        UpdateNoteRecord request,
        CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(activityId);
        Guard.Against.NullOrEmptyGuid(noteId);

        var note = await _repo.GetById(noteId, ct)
            ?? throw new NotFoundException("Note with id: {Id} could not be found", noteId);

        if (note.ActivityId != activityId)
            throw new NotFoundException("Note not found for activity.");

        note.Title = request.Title;
        note.Content = request.Content;

        var updated = await _repo.Update(note, ct);

        return updated.Id;
    }

    public async Task Delete(
        Guid activityId,
        Guid noteId,
        CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(activityId);
        Guard.Against.NullOrEmptyGuid(noteId);

        var note = await _repo.GetById(noteId, ct)
            ?? throw new NotFoundException("Note with id: {Id} could not be found", noteId);

        if (note.ActivityId != activityId)
            throw new NotFoundException("Note not found for activity.");

        await _repo.Delete(noteId, ct);
    }
}