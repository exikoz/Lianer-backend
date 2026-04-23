using Lianer.Core.API.DTOs.User;

public interface IUserService
{
    Task<Guid> Create(CreateUserRequest request, CancellationToken ct);
    Task Delete(Guid Id, CancellationToken ct);
    Task<UserDetails> GetUserById(Guid Id);
    Task<UserSummary> GetUserSummaryById(Guid Id);
    Task<IEnumerable<UserSummary>> GetAllUserSummaries();
    Task<Guid> Update(UpdateUserRequest request, CancellationToken ct);
    Task<Guid> UpdatePassword(UserService.UpdatePasswordRequest request, CancellationToken ct);
}

