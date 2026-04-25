using Microsoft.EntityFrameworkCore;
using Lianer.Core.API.Data;

public sealed class NoteQueryService(AppDbContext ctx) : INoteQueryService
{
    public async Task<IReadOnlyList<NoteSummary>> GetByActivityId(
        Guid activityId,
        CancellationToken ct)
    {
        return await ctx.Notes
            .AsNoTracking()
            .Where(x => x.ActivityId == activityId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NoteSummary(
                x.Id,
                x.ActivityId,
                x.Title,
                x.Content,
                x.CreatedBy,
                x.CreatedAt))
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
            .Select(x => new NoteSummary(
                x.Id,
                x.ActivityId,
                x.Title,
                x.Content,
                x.CreatedBy,
                x.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }
}