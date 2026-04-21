using System.ComponentModel.DataAnnotations;

namespace Lianer.Features.API.Models;

/// <summary>
/// Contact model local to the Features microservice schema
/// </summary>
public class Contact
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string Position { get; set; } = string.Empty;
    
    public string Organization { get; set; } = string.Empty;
    
    /// <summary>
    /// E.g., "Hunter.io" or "Manual"
    /// </summary>
    public string Source { get; set; } = "Unknown";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
