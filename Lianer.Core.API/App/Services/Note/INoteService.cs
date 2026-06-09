public interface INoteService
{
    Task<Guid> Create(Guid activityId,Guid createdBy, CreateNoteRecord request, CancellationToken ct);
    Task Delete(Guid activityId, Guid noteId, CancellationToken ct);
    Task<Guid> Update(Guid activityId, Guid noteId, UpdateNoteRecord request, CancellationToken ct);
}
