public record UserDetails(Guid Id, string FullName, string Email, DateTime CreatedAt, bool IsActive, string Provider, string? ExternalProviderId);
