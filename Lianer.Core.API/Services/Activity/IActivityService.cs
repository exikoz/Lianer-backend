public interface IActivityService
{
    Task<Guid> Create(CreateActivityRecord request, CancellationToken ct);
    Task Delete(Guid Id, CancellationToken ct);

    Task<Guid> Update(UpdateActivityRecord request, CancellationToken ct);

    Task<ActivitySummary?> GetActivityById(Guid id, CancellationToken ct);
}

