public interface IActivityService
{
    Task<Guid> Create(CreateActivityRecord request, CancellationToken ct);
    Task Delete(Guid Id, CancellationToken ct);
    Task<IReadOnlyList<Activity>> GetLatestActivitiesByUserId(Guid userId, int currentPage, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Activity>> GetLatestUpdatedActivities(int currentPage, int pageSize, CancellationToken ct);
    Task<Guid> Update(UpdateActivityRecord request, CancellationToken ct);
}

