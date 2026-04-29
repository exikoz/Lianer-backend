using Lianer.Core.API.App.DTOs.User;
using Lianer.Core.API.DTOs.User;

/// <summary>
/// Service interface for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="request">The user creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the newly created user.</returns>
    Task<Guid> Create(CreateUserRequest request, CancellationToken ct);

    /// <summary>
    /// Deletes a user from the system by their ID.
    /// </summary>
    /// <param name="Id">The unique identifier of the user to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task Delete(Guid Id, CancellationToken ct);

    /// <summary>
    /// Retrieves detailed information about a specific user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <returns>The user details.</returns>
    Task<UserDetails> GetUserById(Guid Id, CancellationToken ct);

    /// <summary>
    /// Retrieves a basic summary of a specific user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <returns>The user summary.</returns>
    Task<UserSummary> GetUserSummaryById(Guid Id, CancellationToken ct);

    /// <summary>
    /// Retrieves summaries for all registered users.
    /// </summary>
    /// <returns>A collection of user summaries.</returns>
    Task<IEnumerable<UserSummary>> GetAllUserSummaries(  CancellationToken ct);

    /// <summary>
    /// Updates an existing user's profile information.
    /// </summary>
    /// <param name="request">The updated user data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the updated user.</returns>
    Task<Guid> Update(UpdateUserRequest request, CancellationToken ct);

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    /// <param name="request">The password update details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the user.</returns>
    Task<Guid> UpdatePassword(UpdatePasswordRequest request, CancellationToken ct);
}

