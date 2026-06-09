public sealed record UpdateActivityRecord(
    Guid Id,
    string? Title,
    string Description,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    ActivityStatus? Status
);
