using System.Collections.Frozen;
using Lianer.Core.API.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;



public class ActivityRepository(AppDbContext context) : ACrud<Activity>(context),  IActivityRepository
{
    protected readonly AppDbContext _c = context;


    public async Task<IReadOnlyList<Activity>> GetLastUpdatedActivities(int currentPage, int pageSize, CancellationToken ct)
    {
        return await _c.Activities
        .AsNoTracking()
        .OrderByDescending(t => t.UpdatedAt)
        .Paginate(currentPage, pageSize)
        .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Activity>> GetActivitiesByUser(Guid userId, int currentPage, int pageSize, CancellationToken ct)
    {
        return await _c.Activities
        .AsNoTracking()
        .Where(t => t.AssignedTo == userId)
        .Paginate(currentPage, pageSize)
        .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Activity>> GetUserActivitiesByDeadline(Guid userId, int currentPage, int pageSize, CancellationToken ct)
    {
        return await _c.Activities
        .AsNoTracking()
        .Where(a => a.AssignedTo == userId)
        .OrderBy(a => a.Deadline)
        .Paginate(currentPage, pageSize)
        .ToListAsync(ct);
    }


}
