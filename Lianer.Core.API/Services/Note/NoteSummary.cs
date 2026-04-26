public sealed record NoteSummary(
    Guid Id,
    Guid ActivityId,
    string Title,
    string Content,
    Guid CreatedBy,
    DateTime CreatedAt
);
