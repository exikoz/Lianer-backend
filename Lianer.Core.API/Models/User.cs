using System.ComponentModel.DataAnnotations;

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
    public string FullName => $"{FirstName} {LastName}";
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
    public User(string firstName, string lastName, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Provider = "Local";
    }

    public void UpdateProfile(string? firstName, string? lastName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName;
        if (!string.IsNullOrWhiteSpace(lastName)) LastName = lastName;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdatePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }
    public static User CreateExternal(string firstName, string lastName, string email, string provider, string externalId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Provider = provider,
            ExternalProviderId = externalId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
