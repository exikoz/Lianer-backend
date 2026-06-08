using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Lianer.Core.API.App.Services.Agent;

public class GeminiService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiService> logger) : IGeminiService
{
    private const string ModelEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    // System prompt that instructs Gemini how to behave as a CRM agent
    private const string SystemPrompt = """
        You are a CRM assistant for the Lianer application.
        Your job is to help the user manage contacts.

        When the user asks you to perform an action (create, read, update, delete a contact),
        you MUST respond ONLY with a valid JSON object — no markdown, no code fences, no extra text.

        The JSON must follow this schema:
        {
          "reply": "<friendly human-readable message describing what you understood>",
          "action": "<one of: CreateContact | GetContact | UpdateContact | DeleteContact | ListContacts | Clarify>",
          "payload": {
            // For CreateContact: firstName, lastName, role, company, email (array), phone (array)
            // For GetContact / DeleteContact: id (guid string) OR name (string to search by)
            // For UpdateContact: id (guid string), and any fields to change
            // For ListContacts: empty object {}
            // For Clarify: question (string with what you need to know)
          }
        }

        Rules:
        - Always set "action" to one of the values above (case-sensitive).
        - If you are unsure or need more info, use action "Clarify" and ask a specific question in payload.question.
        - Never invent GUIDs — if you need an id and don't have it, use action "Clarify".
        - If the user is just chatting (not asking for a CRM action), use action "Clarify" with a helpful reply.
        - All field names in payload must be camelCase.
        - Respond ONLY with the JSON object. No markdown, no explanation outside the JSON.
        """;

    public async Task<AgentChatResponse> ChatAsync(AgentChatRequest request, CancellationToken ct)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogError("Gemini API key is not configured");
            return Fallback("AI-tjänsten är inte konfigurerad just nu. Kontakta administratören.");
        }

        try
        {
            var geminiRequest = BuildGeminiRequest(request);
            var json = JsonSerializer.Serialize(geminiRequest);

            var client = httpClientFactory.CreateClient("Gemini");
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{ModelEndpoint}?key={apiKey}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(httpRequest, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Gemini API returned {Status}: {Body}", response.StatusCode, responseBody);
                return Fallback("AI-tjänsten svarade inte som förväntat. Försök igen.");
            }

            return ParseGeminiResponse(responseBody);
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Gemini request timed out");
            return Fallback("AI-tjänsten tog för lång tid att svara. Försök igen.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error calling Gemini API");
            return Fallback("Ett oväntat fel uppstod. Försök igen om en stund.");
        }
    }

    // ── Private helpers ──────────────────────────────────────────────

    private static object BuildGeminiRequest(AgentChatRequest request)
    {
        var contents = new List<object>();

        // Add conversation history
        foreach (var msg in request.History)
        {
            contents.Add(new
            {
                role = msg.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = msg.Content } }
            });
        }

        // Add current message
        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = request.Message } }
        });

        return new
        {
            system_instruction = new
            {
                parts = new[] { new { text = SystemPrompt } }
            },
            contents,
            generationConfig = new
            {
                temperature = 0.2,          // low temp = more deterministic JSON
                maxOutputTokens = 512,
                responseMimeType = "application/json"
            }
        };
    }

    private AgentChatResponse ParseGeminiResponse(string responseBody)
    {
        try
        {
            var root = JsonNode.Parse(responseBody);
            var text = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogWarning("Gemini returned empty text");
                return Fallback("AI-svaret var tomt. Försök formulera om din fråga.");
            }

            // Parse the structured JSON the model returned
            var parsed = JsonNode.Parse(text);
            if (parsed is null)
            {
                logger.LogWarning("Gemini returned non-JSON text: {Text}", text);
                return Fallback("AI-svaret kunde inte tolkas. Försök igen.");
            }

            var reply = parsed["reply"]?.GetValue<string>() ?? "OK";
            var actionStr = parsed["action"]?.GetValue<string>() ?? "Clarify";
            var payloadNode = parsed["payload"] as JsonObject;

            if (!Enum.TryParse<AgentActionType>(actionStr, out var action))
                action = AgentActionType.Clarify;

            // Build payload dictionary
            var payload = new Dictionary<string, object?>();
            if (payloadNode is not null)
            {
                foreach (var kv in payloadNode)
                {
                    payload[kv.Key] = kv.Value?.GetValue<object>();
                }
            }

            var proposal = action == AgentActionType.Clarify
                ? null
                : new AgentProposal
                {
                    Action = action,
                    Description = reply,
                    Payload = payload
                };

            return new AgentChatResponse { Reply = reply, Proposal = proposal };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gemini response: {Body}", responseBody);
            return Fallback("Kunde inte tolka AI-svaret. Försök igen.");
        }
    }

    private static AgentChatResponse Fallback(string message) =>
        new() { Reply = message, Proposal = null };
}
