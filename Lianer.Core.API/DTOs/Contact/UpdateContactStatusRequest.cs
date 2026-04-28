using Lianer.Core.API.Models;

public record UpdateContactStatusRequest(Guid Id, ContactStatus Status);
