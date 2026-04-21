public interface IActivityRepository : ICrud<Activity>
{
    Task<IReadOnlyList<Activity>> GetActivitiesByUser(Guid userId, int currentPage, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Activity>> GetLastUpdatedActivities(int currentPage, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Activity>> GetUserActivitiesByDeadline(Guid userId, int currentPage, int pageSize, CancellationToken ct);
}