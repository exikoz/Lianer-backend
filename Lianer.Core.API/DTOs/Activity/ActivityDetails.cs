public sealed record ActivityDetails(
    Guid Id,
    string Description,
    Guid? AssignedTo,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? UpdatedAt,
    Guid? NoteId,
    ActivityStatus Status
);
