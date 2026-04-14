namespace Lianer.Core.API.Models;

/// <summary>
/// Internal database model for a CRM contact.
/// </summary>
public class ContactItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public List<string> Phone { get; set; } = [];
    public List<string> Email { get; set; } = [];
    public ContactSocial Social { get; set; } = new();
    public string Status { get; set; } = "Ej kontaktad";
    public string? AssignedTo { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<InteractionLogEntry> InteractionLog { get; set; } = [];
}

public class ContactSocial
{
    public string? LinkedIn { get; set; }
    public string? Website { get; set; }
}

public class InteractionLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Note { get; set; } = string.Empty;
}
