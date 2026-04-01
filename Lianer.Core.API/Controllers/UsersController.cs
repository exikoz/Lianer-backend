using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller för användarhantering (RESTful resource: users)
/// </summary>
[ApiController]
[Route("api/v1/users")]
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
    /// Skapar ett nytt användarkonto (registrering)
    /// </summary>
    /// <param name="request">Användardata (namn, e-post, lösenord)</param>
    /// <returns>Skapad användare</returns>
    /// <response code="201">Användare skapad framgångsrikt</response>
    /// <response code="400">Ogiltig input eller e-post redan registrerad</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponseDto>> CreateUser([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/users anropad");

        var response = await _authService.RegisterAsync(request);

        return CreatedAtAction(
            nameof(GetUser),
            new { id = response.UserId },
            response
        );
    }

    /// <summary>
    /// Hämtar en specifik användare (placeholder för framtida implementation)
    /// </summary>
    /// <param name="id">Användar-ID</param>
    /// <returns>Användare</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisterResponseDto>> GetUser(Guid id)
    {
        // TODO: Implementera i framtida ticket
        _logger.LogInformation("GET /api/v1/users/{Id} anropad", id);
        return NotFound(new { message = "Endpoint ej implementerad än" });
    }
}
