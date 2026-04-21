public sealed record ActivitySummary(
    Guid Id,
    string Description,
    Guid? AssignedTo,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    ActivityStatus Status
);
