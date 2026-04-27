public interface INoteQueryService
{
    Task<IReadOnlyList<NoteSummary>> GetByActivityId(Guid activityId,int currentPage,int pageSize, CancellationToken ct);
    Task<NoteSummary?> GetById(Guid activityId, Guid noteId, CancellationToken ct);
}
