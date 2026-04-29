using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lianer.Core.API.Models;

/// <summary>
/// User entity for database
/// </summary>
public class User
{
    /// <summary>
    /// User's unique ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();    
    /// <summary>
    /// User's email address (unique)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (BCrypt) - nullable for external providers
    /// </summary>
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    /// <summary>
    /// External provider ID (e.g., Google user ID)
    /// </summary>
    [MaxLength(255)]
    public string? ExternalProviderId { get; set; }

    /// <summary>
    /// Authentication provider (Local, Google, Microsoft, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// Timestamp when the account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the account was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates if the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;


    protected User(){}
    public User(string fName, string lName, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        FirstName = fName;
        LastName = lName;
        PasswordHash = passwordHash;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Provider = "Local";
    }

    public void UpdateProfile(string? fName, string? lName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(fName)) FirstName = fName;
        if (!string.IsNullOrWhiteSpace(lName)) LastName = lName;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdatePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }
    public static User CreateExternal(string fName, string email, string provider, string externalId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = fName,
            LastName =  "external", //TODO temporary fix
            Email = email,
            Provider = provider,
            ExternalProviderId = externalId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
