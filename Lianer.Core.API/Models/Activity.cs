using Lianer.Core.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

public class Activity
{
    public Guid Id { get; set; }

    public required string Description { get; set; }

    // FK to UserId
    public Guid? AssignedTo { get; set; }

    public required Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    // if a note is attached, FK to Note
    public Guid? NoteId { get; set; }

    public Note? Note { get; set; } 

    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;



    protected Activity(){}
    public Activity(
        string description,
        Guid? assignedTo,
        Guid creatorId,
        DateTime? startDate,
        DateTime? endDate,
        ActivityStatus status = ActivityStatus.Pending)
    {
        Id = Guid.NewGuid();
        Description = description;
        AssignedTo = assignedTo;
        CreatedBy = creatorId;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    public void UpdateActivity(
        string? description, Guid? assignedTo, 
        DateTime? startDate, DateTime? endDate, 
        ActivityStatus? status)
    {
        if(!string.IsNullOrWhiteSpace(description)) Description = description;
        if(assignedTo != null) AssignedTo = assignedTo;
        if(startDate != null) StartDate = startDate.Value;
        if(endDate != null) EndDate = endDate.Value;
        if(status != null) Status = status.Value;

    }


}