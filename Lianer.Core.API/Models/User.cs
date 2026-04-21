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
}
