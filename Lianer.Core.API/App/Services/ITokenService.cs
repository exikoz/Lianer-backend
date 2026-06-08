using Lianer.Core.API.Models;

namespace Lianer.Core.API.Services;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Gets the token expiration time in seconds
    /// </summary>
    /// <returns>Expiration time in seconds</returns>
    int GetTokenExpirationSeconds();
}