public sealed record CreateActivityRecord(
    string Description,
    Guid? AssignedTo,
    Guid CreatedBy,
    DateTime? StartDate,
    DateTime? EndDate,
    ActivityStatus? Status
);
