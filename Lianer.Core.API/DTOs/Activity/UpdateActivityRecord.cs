public sealed record UpdateActivityRecord(
    Guid Id,
    string? Description,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    ActivityStatus? Status
);
