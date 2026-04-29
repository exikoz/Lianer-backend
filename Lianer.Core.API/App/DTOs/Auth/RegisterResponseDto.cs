namespace Lianer.Core.API.DTOs.Auth;

/// <summary>
/// Response DTO for created user (POST /api/v1/users)
/// </summary>
public class RegisterResponseDto
{
    /// <summary>
    /// User's unique ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
