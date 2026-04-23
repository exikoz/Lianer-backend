namespace Lianer.Features.API.DTOs.Integration;

/// <summary>
/// Data contract for user summary information received from Core API
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="FullName">The full name of the user</param>
/// <param name="Email">The user's email address</param>
public record CoreUserSummaryDto(Guid UserId, string FullName, string Email);
