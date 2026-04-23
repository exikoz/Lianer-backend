using Lianer.Core.API.Common;
using Lianer.Core.API.Models;   
using Lianer.Core.API.DTOs.User;

public class UserService(UserRepository r) : IUserService
{
    private readonly UserRepository _r = r;

    #region write functions
    public async Task<Guid> Create(CreateUserRequest request, CancellationToken ct)
    {
        ValidationHelper(request);

        var user = new User
        (
            request.FirstName,
            request.LastName,
            request.Email,
            HashPassword(request.Password)
        );

        var created = await _r.Create(user, ct);
        return created.Id;
    }

    public async Task Delete(Guid Id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        await _r.Delete(Id, ct);
    }

    public async Task<Guid> Update(UpdateUserRequest request, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(request.Id);
        var user = await _r.GetById(request.Id) ?? throw new NotFoundException("User with id: {Id} could not be found", request.Id);
        user.UpdateProfile(request.FirstName, request.LastName, request.Email);
        var updated = await _r.Update(user, ct);
        return updated.Id;
    }

    public record UpdatePasswordRequest(Guid Id, string Password);
    public async Task<Guid> UpdatePassword(UpdatePasswordRequest request, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(request.Id);
        var user = await _r.GetById(request.Id) ?? throw new NotFoundException("User with id: {Id} could not be found", request.Id);
        var hashed = HashPassword(request.Password);
        user.UpdatePassword(hashed);
        var updated = await _r.Update(user, ct);
        return updated.Id;
    }
    #endregion

    #region read functions

    public async Task<UserDetails> GetUserById(Guid Id)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        var user = await _r.GetById(Id);
        return user != null ? new UserDetails(user.Id, user.FirstName, user.LastName, user.Email, user.CreatedAt, user.IsActive, user.Provider) : throw new NotFoundException("User with id: {Id} could not be found or fetched", Id);
    }

    public async Task<UserSummary> GetUserSummaryById(Guid Id)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        var user = await _r.GetUserSummaryById(Id);
        return user ?? throw new NotFoundException("User with id: {Id} could not be found or fetched", Id);
    }
    #endregion

    #region Helpers
    private string HashPassword(string password)
    => BCrypt.Net.BCrypt.HashPassword(password);

    private static void ValidationHelper(CreateUserRequest request)
    {
        Guard.Against.Null(request);
        Guard.Against.NullOrWhiteSpace(request.Email);
        Guard.Against.NullOrWhiteSpace(request.FirstName);
        Guard.Against.NullOrWhiteSpace(request.LastName);
        Guard.Against.NullOrWhiteSpace(request.Password);
    }


    #endregion
}

