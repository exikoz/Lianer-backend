using System.ComponentModel.DataAnnotations;

namespace Lianer.Core.API.Models;

/// <summary>
/// Användarentitet för databasen
/// </summary>
public class User
{
    /// <summary>
    /// Användarens unika ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Användarens fullständiga namn
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Användarens e-postadress (unik)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashat lösenord (BCrypt)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Tidpunkt när kontot skapades
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Tidpunkt när kontot senast uppdaterades
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indikerar om kontot är aktivt
    /// </summary>
    public bool IsActive { get; set; } = true;
}
