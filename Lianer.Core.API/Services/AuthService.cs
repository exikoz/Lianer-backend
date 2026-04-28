using Lianer.Core.API.Data;
using Microsoft.EntityFrameworkCore;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service for authentication and user management
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, ILogger<AuthService> logger, ITokenService tokenService)
    {
        _context = context;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Creates a new user (POST /api/v1/users)
    /// </summary>
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Normalize email
        var email = request.Email.ToLowerInvariant();

        // Check if email already exists
        var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
        if (emailExists)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", email);
            throw new InvalidOperationException("Email address is already registered");
        }

        // Hash password with BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email.ToLowerInvariant(),
            passwordHash
        );

        // Save to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created successfully: {UserId}", user.Id);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Authenticates a user and creates a session (POST /api/v1/sessions)
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Normalize email
        var email = request.Email.ToLowerInvariant();

        // Retrieve user from database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found with email {Email}", email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogInformation("User found for login: {UserId}, checking password...", user.Id);

        // Verify password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: Invalid password for email {Email}", email);
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

        var emailNormalized = googleUser.Email.ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailNormalized && u.Provider == "Google");

        if (user == null)
        {
            // Auto-register new user if they don't exist
            _logger.LogInformation("Auto-registering new Google user: {Email}", googleUser.Email);
            
            user = User.CreateExternal(
                googleUser.Name,
                emailNormalized,
                "Google",
                googleUser.Id
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("Existing Google user found: {UserId}", user.Id);
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