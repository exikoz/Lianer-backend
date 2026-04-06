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
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ILogger<SessionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new session (login) - To be implemented in K-101
    /// </summary>
    /// <returns>JWT token</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSession()
    {
        // TODO: Implement in K-101
        _logger.LogInformation("POST /api/v1/sessions called (not implemented)");
        return StatusCode(501, new { message = "Login functionality will be implemented in K-101" });
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
}
