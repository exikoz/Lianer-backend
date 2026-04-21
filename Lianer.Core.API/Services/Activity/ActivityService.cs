using System.Collections.ObjectModel;
using Lianer.Core.API.Common;

public interface IActivityService{}

public class ActivityService(IActivityRepository repo)
{
    private readonly IActivityRepository _repo = repo;

    public async Task<IReadOnlyList<Activity>> GetLatestTasks(int currentPage, int pageSize, CancellationToken ct)
    {
        
    }

    private void ValidationHelper(CreateActivityRequest request)
    {
        Guard.Against.Null(request);
        Guard.Against.NullOrWhiteSpace(request.Email);
        Guard.Against.NullOrWhiteSpace(request.FirstName);
        Guard.Against.NullOrWhiteSpace(request.LastName);
        Guard.Against.NullOrWhiteSpace(request.Password);
    }
}