using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for updating an existing user's profile information.
/// </summary>
public record UpdateUserRequest
(
    [Required]
     Guid Id,
    string? FirstName,
    string? LastName,
    [EmailAddress]
    string? Email
);