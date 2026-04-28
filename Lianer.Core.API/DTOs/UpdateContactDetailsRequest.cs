using Lianer.Core.API.Models;

public record UpdateContactDetailRequest(Guid Id, string FirstName, string LastName, string Role,  List<string> Phone, List<string> Email, ContactSocial Social);
public record UpdateContactAssignedTo(Guid Id, Guid AssignedTo);
public record UpdateContactFavorite(Guid Id, bool IsFavorite);
public record UpdateContactDates(Guid Id, DateTime CompletedAt, DateTime LastContactDate);
public record UpdateInteractionLog(Guid ContactId, Guid InteractionLogId, string? Type, string? Content, string? PreviousStatus, string? NewStatus);