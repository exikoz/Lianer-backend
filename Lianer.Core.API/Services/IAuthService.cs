using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Services;

/// <summary>
/// Interface for authentication services
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a new user (POST /api/v1/users)
    /// </summary>
    /// <param name="request">User data</param>
    /// <returns>Created user</returns>
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
}
