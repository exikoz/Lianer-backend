using Lianer.Core.API.Models;

namespace Lianer.Core.API.DTOs;

/// <summary>
/// Response DTO returned when reading a contact.
/// </summary>
public class ContactResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public List<string> Phone { get; set; } = [];

    public List<string> Email { get; set; } = [];

    public ContactSocialDto? Social { get; set; }

    public ContactStatus Status { get; set; } = ContactStatus.EjKontaktad;

    public Guid? AssignedTo { get; set; }

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
    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }
}