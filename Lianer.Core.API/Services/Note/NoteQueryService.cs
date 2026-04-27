using Microsoft.EntityFrameworkCore;
using Lianer.Core.API.Data;

public sealed class NoteQueryService(AppDbContext ctx) : INoteQueryService
{
    public async Task<IReadOnlyList<NoteSummary>> GetByActivityId(
        Guid activityId,
        int currentPage,
        int pageSize,
        CancellationToken ct)
    {
        return await ctx.Notes
            .AsNoTracking()
            .Where(x => x.ActivityId == activityId)
            .OrderByDescending(x => x.CreatedAt)
            .Paginate(currentPage, pageSize)
            .ProjectTo<Note,NoteSummary>()
            .ToListAsync(ct);
    }

 
    public async Task<NoteSummary?> GetById(
        Guid activityId,
        Guid noteId,
        CancellationToken ct)
    {
        return await ctx.Notes
            .AsNoTracking()
            .Where(x => x.ActivityId == activityId && x.Id == noteId)
            .ProjectTo<Note,NoteSummary>()
            .FirstOrDefaultAsync(ct);
    }
}