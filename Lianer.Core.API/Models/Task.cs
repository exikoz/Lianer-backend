using Microsoft.AspNetCore.Http.HttpResults;

public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    OnHold = 4
}

public class TaskItem
{
    public Guid Id { get; set; }

    public required string Description { get; set; }

    // FK to UserId
    public Guid? AssignedTo { get; set; }

    public required string CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime Deadline { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // if a note is attached, FK to Note
    public Guid? NoteId { get; set; }

    public Note? Note { get; set; } = null!;

    public TaskStatus Status { get; set; } = TaskStatus.Pending;
}