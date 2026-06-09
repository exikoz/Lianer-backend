using Lianer.Core.API.DTOs;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.App.Services.Agent;

/// <summary>
/// Executes a confirmed AgentProposal by calling the appropriate service.
/// </summary>
public class AgentExecutor(
    IContactService contactService,
    ILogger<AgentExecutor> logger) : IAgentExecutor
{
    public async Task<AgentConfirmResponse> ExecuteAsync(AgentProposal proposal, CancellationToken ct)
    {
        try
        {
            return proposal.Action switch
            {
                AgentActionType.CreateContact  => await CreateContactAsync(proposal, ct),
                AgentActionType.GetContact     => await GetContactAsync(proposal, ct),
                AgentActionType.UpdateContact  => await UpdateContactAsync(proposal, ct),
                AgentActionType.DeleteContact  => await DeleteContactAsync(proposal, ct),
                AgentActionType.ListContacts   => ListContactsNotSupported(),
                _ => new AgentConfirmResponse { Success = false, Message = "Okänd åtgärd." }
            };
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("Agent executor: not found — {Message}", ex.Message);
            return new AgentConfirmResponse { Success = false, Message = ex.Message };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Agent executor failed for action {Action}", proposal.Action);
            return new AgentConfirmResponse
            {
                Success = false,
                Message = "Ett fel uppstod när åtgärden skulle utföras."
            };
        }
    }

    // ── Action handlers ──────────────────────────────────────────────

    private async Task<AgentConfirmResponse> CreateContactAsync(AgentProposal proposal, CancellationToken ct)
    {
        var p = proposal.Payload;

        var request = new CreateContactRequest
        {
            FirstName = GetString(p, "firstName"),
            LastName  = GetString(p, "lastName"),
            Role      = GetString(p, "role"),
            Company   = GetString(p, "company"),
            Email     = GetStringList(p, "email"),
            Phone     = GetStringList(p, "phone"),
            Status    = ContactStatus.EjKontaktad
        };

        var id = await contactService.Create(request, ct);
        logger.LogInformation("Agent created contact {Id} via AI proposal", id);

        return new AgentConfirmResponse
        {
            Success    = true,
            Message    = $"Kontakten '{request.FirstName} {request.LastName}' skapades.",
            ResourceId = id
        };
    }

    private async Task<AgentConfirmResponse> GetContactAsync(AgentProposal proposal, CancellationToken ct)
    {
        var id = GetGuid(proposal.Payload, "id");
        if (id == Guid.Empty)
            return new AgentConfirmResponse { Success = false, Message = "Inget kontakt-ID angavs." };

        var contact = await contactService.GetContactById(id, ct);
        if (contact is null)
            return new AgentConfirmResponse { Success = false, Message = $"Kontakten med ID {id} hittades inte." };

        return new AgentConfirmResponse { Success = true, Message = "Kontakt hittad.", Data = contact };
    }

    private async Task<AgentConfirmResponse> UpdateContactAsync(AgentProposal proposal, CancellationToken ct)
    {
        var id = GetGuid(proposal.Payload, "id");
        if (id == Guid.Empty)
            return new AgentConfirmResponse { Success = false, Message = "Inget kontakt-ID angavs för uppdatering." };

        var p = proposal.Payload;
        var request = new UpdateContactRequest(
            FirstName: GetStringOrNull(p, "firstName"),
            LastName:  GetStringOrNull(p, "lastName"),
            Role:      GetStringOrNull(p, "role"),
            Company:   GetStringOrNull(p, "company")
        );

        var updatedId = await contactService.Update(id, request, ct);
        logger.LogInformation("Agent updated contact {Id} via AI proposal", updatedId);

        return new AgentConfirmResponse
        {
            Success    = true,
            Message    = $"Kontakten uppdaterades.",
            ResourceId = updatedId
        };
    }

    private async Task<AgentConfirmResponse> DeleteContactAsync(AgentProposal proposal, CancellationToken ct)
    {
        var id = GetGuid(proposal.Payload, "id");
        if (id == Guid.Empty)
            return new AgentConfirmResponse { Success = false, Message = "Inget kontakt-ID angavs för borttagning." };

        await contactService.Delete(id, ct);
        logger.LogInformation("Agent deleted contact {Id} via AI proposal", id);

        return new AgentConfirmResponse { Success = true, Message = $"Kontakten med ID {id} raderades." };
    }

    private static AgentConfirmResponse ListContactsNotSupported() =>
        new()
        {
            Success = false,
            Message = "Listning av kontakter görs direkt via GET /api/v1/contacts."
        };

    // ── Payload helpers ──────────────────────────────────────────────

    private static string GetString(Dictionary<string, object?> p, string key) =>
        p.TryGetValue(key, out var v) && v is not null ? v.ToString()!.Trim() : string.Empty;

    private static string? GetStringOrNull(Dictionary<string, object?> p, string key) =>
        p.TryGetValue(key, out var v) && v is not null ? v.ToString()!.Trim() : null;

    private static Guid GetGuid(Dictionary<string, object?> p, string key)
    {
        if (p.TryGetValue(key, out var v) && v is not null &&
            Guid.TryParse(v.ToString(), out var id))
            return id;
        return Guid.Empty;
    }

    private static List<string> GetStringList(Dictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null)
            return [];

        if (v is System.Text.Json.Nodes.JsonArray arr)
            return [.. arr.Select(x => x?.GetValue<string>() ?? string.Empty).Where(s => s != string.Empty)];

        // Sometimes Gemini returns a comma-separated string instead of array
        if (v is string s && !string.IsNullOrWhiteSpace(s))
            return [.. s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0)];

        return [];
    }
}
