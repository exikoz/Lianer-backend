namespace Lianer.Core.API.Models;

/// <summary>
/// Internal database model for a CRM contact.
/// </summary>
public class Contact
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Company { get; private set; } = string.Empty;
    public List<string> Phone { get; private set; } = [];
    public List<string> Email { get; private set; } = [];
    public ContactSocial Social { get; private set; } = new();
    public ContactStatus Status { get; private set; } = ContactStatus.EjKontaktad;
    public Guid? AssignedTo { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? LastContactDate { get; private set; }
    public List<InteractionLogEntry> InteractionLog { get; private set; } = [];

    private Contact()
    {
    }

    public static Contact Create(
        string firstName,
        string lastName,
        string role,
        string company,
        List<string>? phone = null,
        List<string>? email = null,
        ContactSocial? social = null,
        ContactStatus status = ContactStatus.EjKontaktad,
        Guid? assignedTo = null,
        bool isFavorite = false,
        DateTime? completedAt = null,
        DateTime? lastContactDate = null)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role.Trim(),
            Company = company.Trim(),
            Phone = phone ?? [],
            Email = email ?? [],
            Social = social ?? new ContactSocial(),
            Status = status,
            AssignedTo = assignedTo,
            IsFavorite = isFavorite,
            CompletedAt = completedAt,
            LastContactDate = lastContactDate
        };
    }

    public void Update(
        Guid id,
        string firstName,
        string lastName,
        string role,
        string company,
        List<string>? phone,
        List<string>? email,
        ContactSocial? social,
        ContactStatus status,
        Guid? assignedTo,
        bool isFavorite,
        DateTime? completedAt,
        DateTime? lastContactDate)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role.Trim();
        Company = company.Trim();
        Phone = phone ?? [];
        Email = email ?? [];
        Social = social ?? new ContactSocial();
        Status = status;
        AssignedTo = assignedTo;
        IsFavorite = isFavorite;
        CompletedAt = completedAt;
        LastContactDate = lastContactDate;
    }
}