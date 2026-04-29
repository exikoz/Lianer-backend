using Lianer.Core.API.Models;

namespace Lianer.Core.API.DTOs;

public record UpdateContactRequest(
    string? FirstName = null,
    string? LastName = null,
    string? Role = null,
    string? Company = null,
    List<string>? Phone = null,
    List<string>? Email = null,
    ContactSocialDto? Social = null,
    ContactStatus? Status = null,
    Guid? AssignedTo = null,
    bool? IsFavorite = null,
    DateTime? CompletedAt = null,
    DateTime? LastContactDate = null
);