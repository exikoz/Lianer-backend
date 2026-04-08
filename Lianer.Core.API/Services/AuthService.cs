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
    private readonly ITokenService _tokenService;

    public AuthService(ILogger<AuthService> logger, ITokenService tokenService)
    {
        _logger = logger;
        _tokenService = tokenService;
    }



    /// <summary>
    /// Authenticates a user and creates a session (POST /api/v1/sessions)
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation("Attempting to authenticate user with email: {Email}", request.Email);

        // TODO: Get user from database
        // var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        // if (user == null)
        // {
        //     throw new UnauthorizedAccessException("Invalid email or password");
        // }

        // TODO: For now, create a mock user for testing
        // This will be replaced with actual database lookup
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), // Mock hash
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Verify password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate JWT token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var expiresIn = _tokenService.GetTokenExpirationSeconds();

        _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = new UserInfoDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            }
        };
    }

    /// <summary>
    /// Authenticates or auto-registers a user via Google SSO
    /// </summary>
    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleUserInfoDto googleUser)
    {
        _logger.LogInformation("Processing Google SSO login for email: {Email}", googleUser.Email);

        // TODO: Check if user exists in database by email and provider
        // var existingUser = await _context.Users
        //     .FirstOrDefaultAsync(u => u.Email == googleUser.Email && u.Provider == "Google");

        User user;

        // For now, create a mock user (will be replaced with database logic)
        var existingUser = (User?)null; // Mock: no existing user

        if (existingUser != null)
        {
            // User exists, update last login
            _logger.LogInformation("Existing Google user found: {UserId}", existingUser.Id);
            user = existingUser;
            
            // TODO: Update last login timestamp
            // existingUser.UpdatedAt = DateTime.UtcNow;
            // await _context.SaveChangesAsync();
        }
        else
        {
            // Auto-register new user from Google
            _logger.LogInformation("Auto-registering new Google user: {Email}", googleUser.Email);
            
            user = new User
            {
                Id = Guid.NewGuid(),
                FullName = googleUser.Name,
                Email = googleUser.Email,
                ExternalProviderId = googleUser.Id,
                Provider = "Google",
                PasswordHash = null, // No password for Google users
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // TODO: Save to database
            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();
        }

        // Generate JWT token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var expiresIn = _tokenService.GetTokenExpirationSeconds();

        _logger.LogInformation("Google SSO authentication successful for user: {UserId}", user.Id);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = new UserInfoDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            }
        };
    }
}