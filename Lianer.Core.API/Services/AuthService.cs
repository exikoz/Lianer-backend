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
        _logger.LogInformation("Attempting to create user with email: {Email}", request.Email);

        // Check if email already exists
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email address is already registered");
        }

        // Hash password with BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var nameParts = (request.FullName ?? "").Split(' ', 2);
        var user = new User(
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : string.Empty,
            request.Email,
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
        _logger.LogInformation("Attempting to authenticate user with email: {Email}", request.Email);

        // Retrieve user from database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

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

        // Check if user exists by email and external provider
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == googleUser.Email && u.Provider == "Google");

        if (user == null)
        {
            // Auto-register new user if they don't exist
            _logger.LogInformation("Auto-registering new Google user: {Email}", googleUser.Email);
            
            var nameParts = (googleUser.Name ?? "").Split(' ', 2);
            user = User.CreateExternal(
                nameParts[0],
                nameParts.Length > 1 ? nameParts[1] : string.Empty,
                googleUser.Email,
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