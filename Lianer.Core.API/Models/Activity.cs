using Lianer.Core.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

public class Activity
{
    public Guid Id { get; set; }

    public required string Description { get; set; }

    // FK to UserId
    public Guid? AssignedTo { get; set; }

    public required Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime Deadline { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // if a note is attached, FK to Note
    public Guid? NoteId { get; set; }

    public Note? Note { get; set; } = null!;

    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;



    protected Activity(){}
    public Activity(string Description, ){}


}