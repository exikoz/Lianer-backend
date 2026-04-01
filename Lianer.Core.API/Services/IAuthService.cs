using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Services;

/// <summary>
/// Interface för autentiseringstjänster
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Skapar en ny användare (POST /api/v1/users)
    /// </summary>
    /// <param name="request">Användardata</param>
    /// <returns>Skapad användare</returns>
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
}
