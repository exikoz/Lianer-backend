using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lianer.Core.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generates a JWT access token for the user
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "LianerBackendSharedDevelopmentSecretKey2026!!";
        var issuer = jwtSettings["Issuer"] ?? "http://localhost:5297";
        var audience = jwtSettings["Audience"] ?? "http://localhost:5297";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("fullName", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("JWT token generated for user: {UserId}", user.Id);
        
        return tokenString;
    }

    /// <summary>
    /// Gets the token expiration time in seconds
    /// </summary>
    public int GetTokenExpirationSeconds()
    {
        return GetTokenExpirationMinutes() * 60;
    }

    private int GetTokenExpirationMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return jwtSettings.GetValue<int>("ExpirationMinutes", 60); // Default 60 minutes
    }
}