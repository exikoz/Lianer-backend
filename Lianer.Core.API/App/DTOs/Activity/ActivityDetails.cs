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
/*
public string Title { get; init; } = string.Empty;
public DateTime? StartDate { get; init; }
public DateTime? EndDate { get; init; }

*/