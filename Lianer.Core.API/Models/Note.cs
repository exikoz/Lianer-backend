using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

public class Note
{
    public Guid Id {get; set;}
    [MaxLength(50)]
    public required string Title {get; set;}
    [MaxLength(100)]
    public required string Content {get; set;}
    // fk to user
    public Guid CreatedBy {get; set;}
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

};