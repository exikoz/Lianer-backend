using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for user management (RESTful resource: users)
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController(IAuthService service,  ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = service;
    private readonly ILogger<AuthController> _logger = logger;




    /// <summary>
    /// Gets a specific user (placeholder for future implementation)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        // TODO: Implement in future ticket
        _logger.LogInformation("GET /api/v1/users/{Id} called", id);
        return NotFound(new { message = "Endpoint not yet implemented" });
    }



}
