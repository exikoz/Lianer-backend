using Lianer.Core.API.Common;

public class ActivityService(IActivityRepository repo) : IActivityService
{
    private readonly IActivityRepository _repo = repo;


    #region Write operations (update, create and delete)
    public async Task<Guid> Create(CreateActivityRecord request, CancellationToken ct)
    {
        ValidationHelper(request);
        var activity = new Activity(
            request.Description,
            request.AssignedTo,
            request.CreatedBy,
            request.StartDate,
            request.EndDate,
            request.Status ?? ActivityStatus.Pending
        );

        var created = await _repo.Create(activity, ct);
        return created.Id;
    }

    public async Task<Guid> Update(UpdateActivityRecord request, CancellationToken ct)
    {
        Guard.Against.Null(request);
        var activity = await _repo.GetById(request.Id, ct) ?? throw new NotFoundException("Activity with id: {Id} could not be found", request.Id);
        activity.UpdateActivity(request.Description, request.AssignedTo, request.StartDate, request.EndDate, request.Status);
        var updated = await _repo.Update(activity, ct);
        return updated.Id;
    }

    public async Task Delete(Guid Id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        await _repo.Delete(Id, ct);
    }

    #endregion

    #region Read functions (paged, get by id etc)


    public async Task<ActivitySummary?> GetActivityById
    (Guid id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(id);
        var response =  await _repo.GetById(id,ct);
        
        return new ActivitySummary
        (
            response.Id, 
            response.Description ?? "",
            response.AssignedTo ?? null,
            response.CreatedBy,
            response.CreatedAt, 
            response.UpdatedAt,
            response.Status
        );

    }   

    #endregion
    private static void ValidationHelper(CreateActivityRecord request)
    {
        Guard.Against.Null(request);
        Guard.Against.NullOrWhiteSpace(request.Description);
        Guard.Against.NullOrEmptyGuid(request.CreatedBy);
    }

}

