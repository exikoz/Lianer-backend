namespace Lianer.Core.API.DTOs.Auth;

/// <summary>
/// Response DTO for successful login (POST /api/v1/sessions)
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserInfoDto User { get; set; } = new();
}

/// <summary>
/// User information included in login response
/// </summary>
public class UserInfoDto
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
}