public interface INoteQueryService
{
    Task<IReadOnlyList<NoteSummary>> GetByActivityId(Guid activityId, CancellationToken ct);
    Task<NoteSummary?> GetById(Guid activityId, Guid noteId, CancellationToken ct);
}
