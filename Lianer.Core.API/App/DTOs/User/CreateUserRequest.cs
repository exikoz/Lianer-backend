using System.ComponentModel.DataAnnotations;

namespace Lianer.Core.API.App.DTOs.User;
public record CreateUserRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);
