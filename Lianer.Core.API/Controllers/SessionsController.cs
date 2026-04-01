using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Controller för sessionshantering (login/logout)
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
    /// Skapar en ny session (login) - Implementeras i K-101
    /// </summary>
    /// <returns>JWT-token</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSession()
    {
        // TODO: Implementera i K-101
        _logger.LogInformation("POST /api/v1/sessions anropad (ej implementerad)");
        return StatusCode(501, new { message = "Login-funktionalitet implementeras i K-101" });
    }

    /// <summary>
    /// Tar bort en session (logout) - Framtida implementation
    /// </summary>
    /// <param name="id">Session-ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        // TODO: Implementera i framtida ticket
        _logger.LogInformation("DELETE /api/v1/sessions/{Id} anropad (ej implementerad)", id);
        return StatusCode(501, new { message = "Logout-funktionalitet implementeras senare" });
    }
}
