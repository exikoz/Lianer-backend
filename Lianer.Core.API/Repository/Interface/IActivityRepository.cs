using Lianer.Core.API.Models;

public interface ITaskRepository : ICrud<Activity>
{
    Task<IReadOnlyList<Activity>>GetMultipleTasksPaged(int currentPage, int pageSize, CancellationToken ct);
}