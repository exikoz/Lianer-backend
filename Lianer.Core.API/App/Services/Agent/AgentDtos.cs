namespace Lianer.Core.API.App.Services.Agent;

/// <summary>
/// A single message in the conversation history.
/// </summary>
public record ConversationMessage(string Role, string Content);

/// <summary>
/// Request sent from the frontend chat UI.
/// </summary>
public record AgentChatRequest
{
    /// <summary>The user's natural-language message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional conversation history for multi-turn context.</summary>
    public List<ConversationMessage> History { get; init; } = [];
}

/// <summary>
/// The AI's parsed intent — what action it wants to perform.
/// </summary>
public enum AgentActionType
{
    Unknown,
    CreateContact,
    GetContact,
    UpdateContact,
    DeleteContact,
    ListContacts,
    Clarify          // AI needs more info
}

/// <summary>
/// Structured proposal returned to the frontend before confirmation.
/// </summary>
public record AgentProposal
{
    public AgentActionType Action { get; init; }

    /// <summary>Human-readable description of what will happen.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The data the AI extracted (contact fields, id, etc.).</summary>
    public Dictionary<string, object?> Payload { get; init; } = [];
}

/// <summary>
/// Response sent back to the frontend after /chat.
/// </summary>
public record AgentChatResponse
{
    /// <summary>The AI's natural-language reply shown in the chat.</summary>
    public string Reply { get; init; } = string.Empty;

    /// <summary>
    /// If the AI parsed a clear action, this is the proposal the user
    /// must accept or reject before anything is persisted.
    /// Null when the AI is just answering a question.
    /// </summary>
    public AgentProposal? Proposal { get; init; }
}

/// <summary>
/// Request sent from the frontend when user clicks Accept / Reject.
/// </summary>
public record AgentConfirmRequest
{
    public AgentProposal Proposal { get; init; } = null!;

    /// <summary>True = execute, false = cancel.</summary>
    public bool Confirmed { get; init; }
}

/// <summary>
/// Result after executing (or rejecting) a proposal.
/// </summary>
public record AgentConfirmResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>The created/updated resource ID when applicable.</summary>
    public Guid? ResourceId { get; init; }

    /// <summary>Read data (e.g. contact details) when the action is a query.</summary>
    public object? Data { get; init; }
}
