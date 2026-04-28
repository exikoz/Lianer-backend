using System.ComponentModel.DataAnnotations;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.DTOs;

/// <summary>
/// Request body for updating an existing contact.
/// </summary>
public record UpdateContactRequest
{
    public Guid Id {get; set;}
    /// <summary>
    /// Contact name. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string LastName { get; set; } = string.Empty;


    /// <summary>
    /// Job title / role. Max 200 characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "Role cannot exceed 200 characters.")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Company name. Max 200 characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "Company cannot exceed 200 characters.")]
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Phone numbers.
    /// </summary>
    public List<string> Phone { get; set; } = [];

    /// <summary>
    /// Email addresses.
    /// </summary>
    public List<string> Email { get; set; } = [];

    /// <summary>
    /// Social links (LinkedIn, website).
    /// </summary>
    public ContactSocialDto? Social { get; set; }

    /// <summary>
    /// Contact status. Must be one of: Ej kontaktad, Pågående, Klar, Förlorad, Återkom.
    /// </summary>
    [RegularExpression(@"^(Ej kontaktad|Pågående|Klar|Förlorad|Återkom)$",
        ErrorMessage = "Status must be one of: Ej kontaktad, Pågående, Klar, Förlorad, Återkom.")]
    public ContactStatus Status { get; set; }

    /// <summary>
    /// Person assigned to this contact.  
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Whether this contact is marked as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime? LastContactDate { get; set; }
}
