using System.Text.Json.Serialization;

namespace Lianer.Features.API.DTOs;

/// <summary>
/// Root response from Hunter.io Domain Search API
/// </summary>
public record HunterResponseDto(
    [property: JsonPropertyName("data")] HunterDataDto Data,
    [property: JsonPropertyName("meta")] HunterMetaDto Meta
);

/// <summary>
/// Data payload containing domain and email information
/// </summary>
public record HunterDataDto(
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("organization")] string Organization,
    [property: JsonPropertyName("emails")] List<HunterEmailDto> Emails
);

/// <summary>
/// Represents a single email entry found by Hunter.io
/// </summary>
public record HunterEmailDto(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("confidence")] int Confidence,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("position")] string? Position,
    [property: JsonPropertyName("department")] string? Department
);

/// <summary>
/// Metadata about the search results
/// </summary>
public record HunterMetaDto(
    [property: JsonPropertyName("results")] int Results
);

/// <summary>
/// Response from Hunter.io Email Finder API (used when searching for a specific person)
/// </summary>
public record HunterEmailFinderResponseDto(
    [property: JsonPropertyName("data")] HunterEmailFinderDataDto Data
);

/// <summary>
/// Data payload for a specific person's email find result
/// </summary>
public record HunterEmailFinderDataDto(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("position")] string? Position,
    [property: JsonPropertyName("organization")] string? Organization,
    [property: JsonPropertyName("confidence")] int Confidence,
    [property: JsonPropertyName("linkedin")] string? Linkedin
);
