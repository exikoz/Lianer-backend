namespace Lianer.Core.API.Models;



/// <summary>
/// Internal database model for a CRM contact.
/// </summary>
public class Contact
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public List<string> Phone { get; set; } = [];
    public List<string> Email { get; set; } = [];
    public ContactSocial Social { get; set; } = new();
    public ContactStatus Status { get; set; } = ContactStatus.EjKontaktad;
    public Guid? AssignedTo { get; set; }
    public bool IsFavorite { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastContactDate { get; set; }
    public List<InteractionLogEntry> InteractionLog { get; set; } = [];
}
