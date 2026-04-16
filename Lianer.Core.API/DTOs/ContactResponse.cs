namespace Lianer.Core.API.DTOs;

/// <summary>
/// Response DTO returned when reading a contact.
/// </summary>
public class ContactResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public List<string> Phone { get; set; } = [];
    public List<string> Email { get; set; } = [];
    public ContactSocialDto? Social { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastContactDate { get; set; }
    public List<InteractionLogEntryDto> InteractionLog { get; set; } = [];
}

/// <summary>
/// DTO for an interaction log entry.
/// </summary>
public class InteractionLogEntryDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
}
