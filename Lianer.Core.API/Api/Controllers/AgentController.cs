using Asp.Versioning;
using Lianer.Core.API.App.Services.Agent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Api.Controllers;

/// <summary>
/// AI agent endpoint — natural language CRM assistant powered by Gemini.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agent")]
[Produces("application/json")]
[Authorize]
public class AgentController(
    IGeminiService geminiService,
    IAgentExecutor agentExecutor,
    ILogger<AgentController> logger) : ControllerBase
{
    /// <summary>
    /// Send a natural-language message to the AI agent.
    /// Returns a human-readable reply and optionally a proposal the user
    /// must accept or reject before any data is modified.
    /// </summary>
    /// <remarks>
    /// The frontend should display the <c>reply</c> in the chat and, if
    /// <c>proposal</c> is present, show Accept / Reject buttons.
    /// Nothing is persisted until the user calls <c>POST /confirm</c>.
    /// </remarks>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(AgentChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentChatResponse>> Chat(
        [FromBody] AgentChatRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        logger.LogInformation("Agent chat request: {Message}", request.Message);

        var response = await geminiService.ChatAsync(request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Confirm or reject an AI proposal.
    /// When <c>confirmed</c> is true the action is executed immediately.
    /// When false the proposal is discarded safely — nothing changes.
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(AgentConfirmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentConfirmResponse>> Confirm(
        [FromBody] AgentConfirmRequest request,
        CancellationToken ct)
    {
        if (request.Proposal is null)
            return BadRequest("Proposal cannot be null.");

        if (!request.Confirmed)
        {
            logger.LogInformation("Agent proposal rejected by user: {Action}", request.Proposal.Action);
            return Ok(new AgentConfirmResponse
            {
                Success = true,
                Message = "Åtgärden avbröts. Inget ändrades."
            });
        }

        logger.LogInformation("Agent proposal accepted: {Action}", request.Proposal.Action);
        var result = await agentExecutor.ExecuteAsync(request.Proposal, ct);
        return Ok(result);
    }
}
