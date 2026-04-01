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
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (unique)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (BCrypt)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

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
