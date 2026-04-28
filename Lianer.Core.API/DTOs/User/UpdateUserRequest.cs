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
    public Guid Id;

    /// <summary>
    /// The updated firstname name of the user.
    /// </summary>
    public string? FirstName;

    /// <summary>
    /// The updated lastname address of the user.
    /// </summary>
    public string? LastName;


    /// <summary>
    /// The updated email address of the user.
    /// </summary>
    [EmailAddress]
    public string? Email;

}
