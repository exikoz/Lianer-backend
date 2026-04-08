using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for user management (RESTful resource: users)
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UserController(IUserService service, UserContext context, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserService _service = service;
    private readonly ILogger<UserController> _logger = logger;

    private readonly UserContext  _context = context;


    [HttpGet]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
    {
        var userId = _context.UserId; 
        if (userId is null) return Unauthorized();
        var user = _service.GetById(userId.Value); // Uses ".value" since GetById doesn't accept nullable id
        return Ok(user);
    }


    /// <summary>
    /// Creates a new user account (registration)
    /// </summary>
    /// <param name="request">User data (name, email, password)</param>
    /// <returns>Created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input or email already registered</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //TODO - extend actionresult?
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/users called");
        var response = await _service.CreateUser(request);
        return CreatedAtAction(
            nameof(GetUser),
            new { id = response.UserId },
            response
        );
    }



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
        _logger.LogInformation("GET /api/v1/users/{Id} called", id);
        return await _service.GetById(id);
    }



}
