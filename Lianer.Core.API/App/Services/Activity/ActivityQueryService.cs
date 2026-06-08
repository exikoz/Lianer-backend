using Lianer.Core.API.Common;
using Lianer.Core.API.Data;
using Microsoft.EntityFrameworkCore;

public sealed class ActivityQueryService(AppDbContext ctx) : IActivityQueryService
{

    /// <summary>
    /// Fetches a paginated IReadOnlyList of activities sorted by UpdatedAt.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<ActivitySummary>> GetLastUpdatedActivities(int currentPage, int pageSize, CancellationToken ct)
    {
        Guard.Against.NegativeOrZero(currentPage);
        Guard.Against.NegativeOrZero(pageSize);
        return await ctx.Activities
        .AsNoTracking()
        .OrderByDescending(t => t.UpdatedAt)
        .Paginate(currentPage, pageSize)
        .ProjectTo<Activity, ActivitySummary>()
        .ToListAsync(ct);
    }

    /// <summary>
    /// Fetches a paginated IReadOnlyList of activities sorted by the user assigned to the activities.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<ActivitySummary>> GetLatestActivitiesByUserId
    (Guid userId, int currentPage, int pageSize, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(userId);
        Guard.Against.NegativeOrZero(currentPage);
        Guard.Against.NegativeOrZero(pageSize);
        return await ctx.Activities
        .AsNoTracking()
        .Where(t => t.AssignedTo == userId)
        .Paginate(currentPage, pageSize)
        .ProjectTo<Activity, ActivitySummary>()
        .ToListAsync(ct);
    }

    /// <summary>
    /// Fetches a paginated IReadOnlyList of activities sorted by the assigned User and deadline.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<ActivitySummary>> GetUserActivitiesByDeadline(Guid userId, int currentPage, int pageSize, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(userId);
        Guard.Against.NegativeOrZero(currentPage);
        Guard.Against.NegativeOrZero(pageSize);

        return await ctx.Activities
        .AsNoTracking()
        .Where(a => a.AssignedTo == userId)
        .OrderBy(a => a.EndDate)
        .Paginate(currentPage, pageSize)
        .ProjectTo<Activity, ActivitySummary>()
        .ToListAsync(ct);
    }


    /// <returns> Returns a summary by activity Id </returns>
    public async Task<ActivitySummary?> GetActivitySummaryById(Guid Id,  CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);

        return await ctx.Activities
        .AsNoTracking()
        .Where(a => a.Id == Id)
        .OrderBy(a => a.EndDate)
        .ProjectTo<Activity, ActivitySummary>()
        .FirstOrDefaultAsync(ct);
    }


}

