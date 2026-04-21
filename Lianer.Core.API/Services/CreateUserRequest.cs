using System.ComponentModel.DataAnnotations;

public record CreateUserRequest
{
    [Required] public string FirstName { get; init; } = default!;
    [Required] public string LastName { get; init; } = default!;
    [Required, EmailAddress] public string Email { get; init; } = default!;
    [Required, MinLength(8)] public string Password { get; init; } = default!;
}
