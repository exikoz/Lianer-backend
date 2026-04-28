public partial class UserService
{
    public record UpdatePasswordRequest(Guid Id, string Password);
}

