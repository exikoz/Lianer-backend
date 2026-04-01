using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service för autentisering och användarhantering
/// </summary>
public class AuthService : IAuthService
{
    // TODO: Injicera DbContext när det är konfigurerat
    // private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Skapar en ny användare (POST /api/v1/users)
    /// </summary>
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        _logger.LogInformation("Försöker skapa användare med e-post: {Email}", request.Email);

        // TODO: Kontrollera om e-post redan finns
        // var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        // if (existingUser != null)
        // {
        //     throw new InvalidOperationException("E-postadressen är redan registrerad");
        // }

        // TODO: Hasha lösenord med BCrypt
        // string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = "TEMP_HASH", // TODO: Ersätt med BCrypt hash
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // TODO: Spara till databas
        // _context.Users.Add(user);
        // await _context.SaveChangesAsync();

        _logger.LogInformation("Användare skapad framgångsrikt: {UserId}", user.Id);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
