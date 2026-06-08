public sealed record CreateNoteRecord(
    string Title,
    string Content,
    Guid CreatedBy
);
