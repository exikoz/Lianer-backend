using System.Linq.Expressions;
using Lianer.Core.API.Projection;

public sealed record NoteSummary(
    Guid Id,
    Guid ActivityId,
    string Title,
    string Content,
    Guid CreatedBy,
    DateTime CreatedAt
) : IProjection<Note, NoteSummary>
{
    public static Expression<Func<Note, NoteSummary>> 
    Selector => x => new NoteSummary(
                x.Id,
                x.ActivityId,
                x.Title,
                x.Content,
                x.CreatedBy,
                x.CreatedAt);
}