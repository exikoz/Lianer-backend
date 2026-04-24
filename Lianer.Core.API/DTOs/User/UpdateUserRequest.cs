using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for updating an existing user's profile information.
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// The unique identifier of the user to update.
    /// </summary>
    [Required]
    public Guid Id { get; init; }

    /// <summary>
    /// The updated full name of the user.
    /// </summary>
    [Required]
    public string FullName { get; init; } = null!;

    /// <summary>
    /// The updated email address of the user.
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; init; } = null!;

    /// <summary>
    /// The optional updated password for the user.
    /// </summary>
    [MinLength(8)]
    public string? Password { get; init; }
}
