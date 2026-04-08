using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Services;

public interface IUserService
{
    /// <summary>
    /// Creates a new user (POST /api/v1/users)
    /// </summary>
    /// <param name="req">User data</param>
    /// <returns>Created user</returns>
    Task<UserResponseDto> CreateUser(UserRequestDto req);
    Task<UserResponseDto> UpdateUser(UserRequestDto req);
    Task<UserResponseDto> GetById(Guid id);
    bool RemoveById(Guid id);


}