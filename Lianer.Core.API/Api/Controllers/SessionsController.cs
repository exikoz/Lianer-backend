using System.Security.Claims;
using Asp.Versioning;
using Lianer.Core.API.App.Auth;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller for session management.
/// Authentication is handled with HttpOnly cookies.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions")]
[Produces("application/json")]
public class SessionsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly AuthCookieService _authCookieService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        IAuthService authService,
        IGoogleAuthService googleAuthService,
        AuthCookieService authCookieService,
        IMemoryCache cache,
        ILogger<SessionsController> logger)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
        _authCookieService = authCookieService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSession([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/sessions called");

        var response = await _authService.LoginAsync(request);

        _authCookieService.CreateAccessCookie(
            Response,
            response.AccessToken);

        _authCookieService.CreateCsrfCookie(Response);

        return Ok(new
        {
            message = "Login successful.",
            user = response.User
        });
    }


    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult DeleteSession()
    {
        _logger.LogInformation("DELETE /api/v1/sessions called");

        _authCookieService.DeleteAuthenticationCookies(Response);

        return NoContent();
    }

    /// <summary>
    /// Returns the current authenticated user from the JWT inside the HttpOnly cookie.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var email =
            User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Email);

        var fullName =
            User.FindFirstValue("fullName")
            ?? User.FindFirstValue(ClaimTypes.Name);

        return Ok(new
        {
            user = new
            {
                userId,
                email,
                fullName
            }
        });
    }

    /// <summary>
    /// Creates a new session via Google SSO.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateGoogleSession([FromBody] GoogleLoginRequestDto request)
    {
        _logger.LogInformation("POST /api/v1/sessions/google called");

        try
        {
            var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.AccessToken);

            if (googleUser == null)
            {
                _logger.LogWarning("Invalid Google access token provided");
                return Unauthorized(new { message = "Invalid Google access token" });
            }

            var response = await _authService.GoogleLoginAsync(googleUser);

            _authCookieService.CreateAccessCookie(
                Response,
                response.AccessToken);

            _authCookieService.CreateCsrfCookie(Response);

            _cache.Remove("users_list");

            return Ok(new
            {
                message = "Google login successful.",
                user = response.User
            });
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

    /// <summary>
    /// Gets the Google OAuth2 authorization URL to start the login flow.
    /// </summary>
    [HttpGet("google/url")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetGoogleUrl()
    {
        _logger.LogInformation("GET /api/v1/sessions/google/url called");

        var url = _googleAuthService.GetGoogleLoginUrl();

        return Ok(new { url });
    }

    /// <summary>
    /// Temporary diagnostic endpoint for verifying that cookie auth works.
    /// Can be removed later.
    /// </summary>
    [HttpGet("cookie-test")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult CookieTest()
    {
        var diagnostics = _authCookieService.GetUserInfoFromCookie(HttpContext);

        _logger.LogInformation("Cookie auth diagnostics: {@Diagnostics}", diagnostics);

        return Ok(new
        {
            message = "Cookie auth works. User extracted from JWT cookie.",
            diagnostics
        });
    }
}