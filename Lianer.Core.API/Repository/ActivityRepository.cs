using System.Collections.Frozen;
using Lianer.Core.API.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public static class QueryExtensions
{
    public static IQueryable<T> Paginate<T>
    (
        this IQueryable<T> query, int currentPage, int pageSize
    )
    {
        return query.Skip((currentPage-1)*pageSize).Take(pageSize);
    }
}

public class ActivityRepository(AppDbContext context) : ACrud<Activity>(context), ITaskRepository
{
    protected readonly AppDbContext _context = context;


    public async Task<IReadOnlyList<Activity>> GetMultipleTasksPaged(int currentPage, int pageSize, CancellationToken ct)
    {
        var tasks = await _context.Tasks
        .AsNoTracking()
        .OrderByDescending(t => t.UpdatedAt)
        .Paginate(currentPage, pageSize)
        .ToListAsync(ct);
        return tasks;
    }
}
