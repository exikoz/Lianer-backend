using System.ComponentModel.DataAnnotations;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.DTOs;

/// <summary>
/// Request body for creating a new contact.
/// </summary>
public record CreateContactRequest
{
    /// <summary>
    /// Contact name. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First Name must be between 1 and 100 characters.")]
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Contact name. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last Name must be between 1 and 100 characters.")]
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
    /// Contact status.
    /// </summary>
    public ContactStatus Status { get; set; } = ContactStatus.EjKontaktad;

    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Whether this contact is marked as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }
}

/// <summary>
/// Social media links for a contact.
/// </summary>
public class ContactSocialDto
{
    /// <summary>
    /// LinkedIn profile URL. Max 500 characters.
    /// </summary>
    [StringLength(500, ErrorMessage = "LinkedIn URL cannot exceed 500 characters.")]
    public string? LinkedIn { get; set; }

    /// <summary>
    /// Website URL. Max 500 characters.
    /// </summary>
    [StringLength(500, ErrorMessage = "Website URL cannot exceed 500 characters.")]
    public string? Website { get; set; }
}
