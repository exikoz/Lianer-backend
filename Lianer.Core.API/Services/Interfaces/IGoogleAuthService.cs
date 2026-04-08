using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Services;

/// <summary>
/// Interface for Google OAuth2 authentication service
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Validates Google access token and retrieves user information
    /// </summary>
    /// <param name="accessToken">Google OAuth2 access token</param>
    /// <returns>Google user information if token is valid</returns>
    Task<GoogleUserInfoDto?> ValidateGoogleTokenAsync(string accessToken);
}