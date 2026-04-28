using System.ComponentModel.DataAnnotations;

namespace Lianer.Core.API.DTOs.Auth;

/// <summary>
/// Request DTO for Google SSO login (POST /api/v1/sessions/google)
/// </summary>
public class GoogleLoginRequestDto
{
    /// <summary>
    /// Google OAuth2 access token received from frontend
    /// </summary>
    [Required(ErrorMessage = "Google access token is required")]
    public string AccessToken { get; set; } = string.Empty;
}