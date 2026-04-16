using Asp.Versioning;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAuthService authService, ILogger<UsersController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user account (registration)
    /// </summary>
    /// <param name="request">User data (name, email, password)</param>
    /// <returns>Created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input or email already registered</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponseDto>> CreateUser([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/users called");

        var response = await _authService.RegisterAsync(request);

        return CreatedAtAction(
            nameof(GetUser),
            new { version = "1.0", id = response.UserId },
            response
        );
    }

    /// <summary>
    /// Gets a specific user (placeholder for future implementation)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisterResponseDto>> GetUser(Guid id)
    {
        // TODO: Implement in future ticket
        _logger.LogInformation("GET /api/v1/users/{Id} called", id);
        return NotFound(new { message = "Endpoint not yet implemented" });
    }
}
