using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for session management (login/logout)
/// </summary>
[ApiController]
[Route("api/v1/sessions")]
[Produces("application/json")]
public class SessionsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        IAuthService authService, 
        IGoogleAuthService googleAuthService,
        ILogger<SessionsController> logger)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new session (login) - Returns JWT token
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid input data</response>
    [HttpPost]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> CreateSession([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/sessions called");

        var response = await _authService.LoginAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Deletes a session (logout) - Future implementation
    /// </summary>
    /// <param name="id">Session ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        // TODO: Implement in future ticket
        _logger.LogInformation("DELETE /api/v1/sessions/{Id} called (not implemented)", id);
        return StatusCode(501, new { message = "Logout functionality will be implemented later" });
    }

    /// <summary>
    /// Creates a new session via Google SSO (auto-registration)
    /// </summary>
    /// <param name="request">Google OAuth2 access token</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="401">Invalid Google token</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="503">Google authentication service unavailable</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<LoginResponseDto>> CreateGoogleSession([FromBody] GoogleLoginRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/sessions/google called");

        try
        {
            // Validate Google access token and get user info
            var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.AccessToken);
            
            if (googleUser == null)
            {
                _logger.LogWarning("Invalid Google access token provided");
                return Unauthorized(new { message = "Invalid Google access token" });
            }

            // Authenticate or auto-register user
            var response = await _authService.GoogleLoginAsync(googleUser);

            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("temporarily unavailable"))
        {
            _logger.LogError(ex, "Google authentication service unavailable");
            return StatusCode(503, new { message = "Google authentication service is temporarily unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google SSO authentication");
            return StatusCode(500, new { message = "Internal server error during Google authentication" });
        }
    }
}