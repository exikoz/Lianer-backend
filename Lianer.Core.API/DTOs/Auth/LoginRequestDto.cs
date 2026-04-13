using System.ComponentModel.DataAnnotations;

namespace Lianer.Core.API.DTOs.Auth;

/// <summary>
/// Request DTO for user login (POST /api/v1/sessions)
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}