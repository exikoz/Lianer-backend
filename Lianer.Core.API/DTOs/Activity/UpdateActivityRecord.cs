public sealed record UpdateActivityRecord(
    string? Description,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    ActivityStatus? Status
);
