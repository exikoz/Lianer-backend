using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Services;

/// <summary>
/// Interface for authentication services
/// </summary>
public interface IAuthService
{
    //Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Authenticates a user and creates a session (POST /api/v1/sessions)
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Authenticates or auto-registers a user via Google SSO
    /// </summary>
    /// <param name="googleUser">Google user information</param>
    /// <returns>JWT token and user information</returns>
    Task<LoginResponseDto> GoogleLoginAsync(GoogleUserInfoDto googleUser);
}
