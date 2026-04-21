using System.ComponentModel.DataAnnotations;

public record UpdateUserRequest
{
    public Guid Id {get; init;}
    public string? FirstName { get; init; } = null!;
    public string? LastName { get; init; } = null!;
    public string? Email { get; init; } = null!;
    [MinLength(8)]public string? Password { get; init; } = null!;
}
