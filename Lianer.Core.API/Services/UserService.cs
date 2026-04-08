using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Models;
using Lianer.Core.API.Common;
using Lianer.Core.API.Repositories;
using Lianer.Core.API.Exceptions.User;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service for authentication and user management
/// </summary>
public class UserService
(ILogger<UserService> logger, 
IUserRepository<User> userRepository
) : IUserService

{
    // TODO: Inject DbContext when configured
    // private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger = logger;
 
    private readonly  IUserRepository<User> _repository = userRepository;


    public async Task<UserResponseDto> GetById(Guid id)
    {
        Guard.Against.NullOrEmptyGuid(id);
        var user =  await _repository.GetById(id);
        return new UserResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

   

    /// <summary>
    /// Creates a new user (POST /api/v1/users)
    /// </summary>
    public async Task<UserResponseDto> CreateUser(UserRequestDto request)
    {
        
        Guard.Against.NullOrWhiteSpace(request.Email);
        Guard.Against.NullOrWhiteSpace(request.FullName);
        Guard.Against.NullOrWhiteSpace(request.Password);
        _logger.LogInformation("Attempting to create user with email: {Email}", request.Email);
        IsEmailTaken(request.Email);

        // TODO: Hash password with BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.Create(user);
        _logger.LogInformation("User created successfully: {UserId}", user.Id);

        return new UserResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    public Task<UserResponseDto> UpdateUser(UserRequestDto req)
    {
        throw new NotImplementedException();
    }

    public bool RemoveById(Guid id)
    {
        throw new NotImplementedException();
    }


    // HELPER FUNCTIONS

    
    //TODO GÖR AWAIT/ASYNC
    private void IsEmailTaken(string username)
    {
        bool result = _repository.IsEmailTaken(username);
        if(result) throw new EmailTakenException();
    }

}