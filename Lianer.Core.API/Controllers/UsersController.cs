using Microsoft.Extensions.Caching.Memory;
using Asp.Versioning;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.DTOs.User;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for user management (RESTful resource: users)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UsersController> _logger;

    private const string UsersListCacheKey = "users_list";
    private static string UserByIdCacheKey(Guid id) => $"user_{id}";

    public UsersController(
        IAuthService authService, 
        IUserService userService, 
        IMemoryCache cache,
        ILogger<UsersController> logger)
    {
        _authService = authService;
        _userService = userService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of all users
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserSummary>>> ListUsers(CancellationToken ct)
    {
        _logger.LogInformation("GET /api/v1/users called (Cache Check)");

        if (_cache.TryGetValue(UsersListCacheKey, out IEnumerable<UserSummary>? cachedUsers))
        {
            _logger.LogInformation("Returning users list from cache.");
            return Ok(cachedUsers);
        }

        _logger.LogInformation("Cache miss. Fetching users from service.");
        var users = await _userService.GetAllUserSummaries(ct);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(UsersListCacheKey, users, cacheOptions);

        return Ok(users);
    }

    /// <summary>
    /// Creates a new user account (registration)
    /// </summary>
    /// <param name="request">User data (name, email, password)</param>
    /// <returns>Created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input or email already registered</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequest request,CancellationToken ct)
    {
        _logger.LogInformation("POST /api/v1/users called");

        var response = await _userService.Create(request,ct);
        
        // Invalidate list cache
        _cache.Remove(UsersListCacheKey);

        return CreatedAtAction(
            nameof(GetUser),
            new { version = "1.0", id = response },
            response
        );
    }

    /// <summary>
    /// Gets a specific user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="ct"></param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSummary>> GetUser(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("GET /api/v1/users/{Id} called (Cache Check)", id);

        var cacheKey = UserByIdCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out UserSummary? cachedUser))
        {
            _logger.LogInformation("Returning user {Id} from cache.", id);
            return Ok(cachedUser);
        }

        _logger.LogInformation("Cache miss for user {Id}. Fetching from service.", id);

        try
        {
            var userSummary = await _userService.GetUserSummaryById(id,ct);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(cacheKey, userSummary, cacheOptions);

            return Ok(userSummary);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
    }

    /// <summary>
    /// Updates an existing user's profile information
    /// </summary>
    /// <param name="id">The unique identifier of the user to update</param>
    /// <param name="request">The updated user data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User updated successfully</response>
    /// <response code="400">Invalid input or ID mismatch</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("PUT /api/v1/users/{Id} called", id);

        if (id != request.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in body" });
        }

        await _userService.Update(request,  ct);

        // Invalidate caches
        _cache.Remove(UsersListCacheKey);
        _cache.Remove(UserByIdCacheKey(id));

        return NoContent();
    }

    /// <summary>
    /// Deletes a user account (only allowed for the user themselves)
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="403">Forbidden - users can only delete their own account</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("DELETE /api/v1/users/{Id} called", id);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var parsedId) || parsedId != id)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to delete user {TargetId}", currentUserId, id);
            return Forbid();
        }

        await _userService.Delete(id, ct);

        // Invalidate caches
        _cache.Remove(UsersListCacheKey);
        _cache.Remove(UserByIdCacheKey(id));

        return NoContent();
    }
}
