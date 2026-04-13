namespace Lianer.Core.API.DTOs.Auth;

/// <summary>
/// DTO representing user information from Google OAuth2 API
/// </summary>
public class GoogleUserInfoDto
{
    /// <summary>
    /// Google user ID (unique identifier)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address from Google
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name from Google
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's given (first) name
    /// </summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>
    /// User's family (last) name
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's profile picture
    /// </summary>
    public string Picture { get; set; } = string.Empty;

    /// <summary>
    /// Whether the email is verified by Google
    /// </summary>
    public bool VerifiedEmail { get; set; }
}