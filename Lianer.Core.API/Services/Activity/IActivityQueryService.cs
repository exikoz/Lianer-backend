public interface IActivityQueryService
{
    Task<IReadOnlyList<ActivitySummary>> GetLastUpdatedActivities(int currentPage, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ActivitySummary>> GetLatestActivitiesByUserId(Guid userId, int currentPage, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ActivitySummary>> GetUserActivitiesByDeadline(Guid userId, int currentPage, int pageSize, CancellationToken ct);

    Task<ActivitySummary?> GetActivitySummaryById(Guid Id, CancellationToken ct);
}

