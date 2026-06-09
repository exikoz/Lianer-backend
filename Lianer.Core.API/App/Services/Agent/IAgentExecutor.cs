namespace Lianer.Core.API.App.Services.Agent;

public interface IAgentExecutor
{
    Task<AgentConfirmResponse> ExecuteAsync(AgentProposal proposal, CancellationToken ct);
}
