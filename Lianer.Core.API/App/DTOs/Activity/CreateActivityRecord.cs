public sealed record CreateActivityRecord(
    
    string? Title,
    string Description,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    ActivityStatus? Status
);

