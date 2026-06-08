namespace Lianer.Features.API.DTOs.Integration;

/// <summary>
/// Data contract for user summary information received from Core API.
/// This matches the UserSummary record in the Core.API microservice.
/// </summary>
public record CoreUserSummaryDto
{
    /// <summary>
    /// The unique identifier of the user (maps to UserId in Core API).
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The full name of the user.
    /// </summary>
    public string FullName { get; init; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; init; } = null!;
}
