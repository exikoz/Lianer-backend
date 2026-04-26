using System.Linq.Expressions;
using Lianer.Core.API.Projection;

public sealed record ActivitySummary(
    Guid Id,
    string Description,
    Guid? AssignedTo,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    ActivityStatus Status
) : IProjection<Activity, ActivitySummary>
{
    public static Expression<Func<Activity, ActivitySummary>> Selector => 
    a => new ActivitySummary(
        a.Id,
        a.Description,
        a.AssignedTo,
        a.CreatedBy,
        a.CreatedAt,
        a.UpdatedAt,
        a.Status
    );
}