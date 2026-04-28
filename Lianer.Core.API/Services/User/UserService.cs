using Lianer.Core.API.Common;
using Lianer.Core.API.Models;   
using Lianer.Core.API.DTOs.User;

public partial class UserService(IUserRepository r) : IUserService
{
    private readonly IUserRepository _r = r;

    #region write functions
    /// <summary>
    /// Creates a new user with the provided details.
    /// </summary>
    /// <param name="request">The creation request containing user data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The GUID of the created user.</returns>
    public async Task<Guid> Create(CreateUserRequest request, CancellationToken ct)
    {
        ValidationHelper(request);

        var user = new User
        (
            request.FirstName,
            request.LastName,
            request.Email.ToLowerInvariant(),
            HashPassword(request.Password)
        );

        var created = await _r.Create(user, ct);
        return created.Id;
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="Id">The GUID of the user to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task Delete(Guid Id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        await _r.Delete(Id, ct);
    }

    /// <summary>
    /// Updates an existing user's profile information.
    /// </summary>
    /// <param name="request">The update request containing new user data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The GUID of the updated user.</returns>
    public async Task<Guid> Update(UpdateUserRequest request, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(request.Id);
        var user = await _r.GetById(request.Id,ct) ?? throw new NotFoundException("User with id: {Id} could not be found", request.Id);
        user.UpdateProfile(request.FirstName ?? user.FirstName, request.LastName ?? user.LastName, request.Email?.ToLowerInvariant() ?? user.Email);
        var updated = await _r.Update(user, ct);
        return updated.Id;
    }
    /// <summary>
    /// Updates a user's password securely.
    /// </summary>
    /// <param name="request">The password update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The GUID of the user whose password was updated.</returns>
    public async Task<Guid> UpdatePassword(UpdatePasswordRequest request, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(request.Id);
        var user = await _r.GetById(request.Id,ct) ?? throw new NotFoundException("User with id: {Id} could not be found", request.Id);
        var hashed = HashPassword(request.Password);
        user.UpdatePassword(hashed);
        var updated = await _r.Update(user, ct);
        return updated.Id;
    }
    #endregion

    #region read functions
    /// <summary>
    /// Retrieves full details for a user by ID.
    /// </summary>
    /// <param name="Id">The user GUID.</param>
    /// <param name="ct"></param>
    /// <returns>A DTO containing user details.</returns>
    public async Task<UserDetails> GetUserById(Guid Id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        var user = await _r.GetById(Id,ct);
        return user != null ? new UserDetails(user.Id, user.FullName, user.Email, user.CreatedAt, user.IsActive, user.Provider, user.ExternalProviderId) : throw new NotFoundException("User with id: {Id} could not be found or fetched", Id);
    }

    /// <summary>
    /// Retrieves a basic summary of a user by ID.
    /// </summary>
    /// <param name="Id">The user GUID.</param>
    /// <returns>A DTO containing a user summary.</returns>
    public async Task<UserSummary> GetUserSummaryById(Guid Id,CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(Id);
        var user = await _r.GetUserSummaryById(Id,ct);
        return user ?? throw new NotFoundException("User with id: {Id} could not be found or fetched", Id);
    }

    /// <summary>
    /// Retrieves summaries for all users in the system.
    /// </summary>
    /// <returns>An enumerable of user summaries.</returns>
    public async Task<IEnumerable<UserSummary>> GetAllUserSummaries(CancellationToken ct)
    {
        return await _r.GetAllUserSummaries(ct);
    }
    #endregion

    #region Helpers
    private string HashPassword(string password)
    => BCrypt.Net.BCrypt.HashPassword(password, 12,true);

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

