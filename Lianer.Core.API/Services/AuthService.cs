using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service for authentication and user management
/// </summary>
public class AuthService : IAuthService
{
    // TODO: Inject DbContext when configured
    // private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user (POST /api/v1/users)
    /// </summary>
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        _logger.LogInformation("Attempting to create user with email: {Email}", request.Email);

        // TODO: Check if email already exists
        // var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        // if (existingUser != null)
        // {
        //     throw new InvalidOperationException("Email address is already registered");
        // }

        // TODO: Hash password with BCrypt
        // string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = "TEMP_HASH", // TODO: Replace with BCrypt hash
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // TODO: Save to database
        // _context.Users.Add(user);
        // await _context.SaveChangesAsync();

        _logger.LogInformation("User created successfully: {UserId}", user.Id);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
