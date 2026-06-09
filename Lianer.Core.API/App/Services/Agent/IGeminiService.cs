namespace Lianer.Core.API.App.Services.Agent;

public interface IGeminiService
{
    /// <summary>
    /// Sends the user message (with history) to Gemini and returns a structured proposal.
    /// </summary>
    Task<AgentChatResponse> ChatAsync(AgentChatRequest request, CancellationToken ct);
}
