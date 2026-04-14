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
}
