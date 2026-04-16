using System.ComponentModel.DataAnnotations;

namespace Lianer.Core.API.DTOs;

/// <summary>
/// Request body for updating an existing contact.
/// </summary>
public class UpdateContactRequest
{
    /// <summary>
    /// Contact name. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

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
    public string Status { get; set; } = "Ej kontaktad";

    /// <summary>
    /// Person assigned to this contact. Max 200 characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "AssignedTo cannot exceed 200 characters.")]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Whether this contact is marked as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }
}
