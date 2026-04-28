using System.ComponentModel.DataAnnotations;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.DTOs;

/// <summary>
/// Request body for updating an existing contact.
/// </summary>
public record UpdateContactRequest
{
    /// <summary>
    /// Contact name. Required, max 200 characters.
    /// </summary>
    public string? FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; } = string.Empty;


    /// <summary>
    /// Job title / role. Max 200 characters.
    /// </summary>
    public string? Role { get; set; } = string.Empty;

    /// <summary>
    /// Company name. Max 200 characters.
    /// </summary>
    public string? Company { get; set; } = string.Empty;

    /// <summary>
    /// Phone numbers.
    /// </summary>
    public List<string>? Phone { get; set; } = [];

    /// <summary>
    /// Email addresses.
    /// </summary>
    public List<string>? Email { get; set; } = [];

    /// <summary>
    /// Social links (LinkedIn, website).
    /// </summary>
    public ContactSocialDto? Social { get; set; }

    /// <summary>
    /// Contact status. Must be one of: Ej kontaktad, Pågående, Klar, Förlorad, Återkom.
    /// </summary>
    //[RegularExpression(@"^(Ejkontaktad|Pågående|Klar|Förlorad|Återkom)$",
        //ErrorMessage = "Status must be one of: Ej kontaktad, Pågående, Klar, Förlorad, Återkom.")]
    public ContactStatus? Status { get; set; }

    /// <summary>
    /// Person assigned to this contact.  
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Whether this contact is marked as a favorite.
    /// </summary>
    public bool? IsFavorite { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime? LastContactDate { get; set; }
}
