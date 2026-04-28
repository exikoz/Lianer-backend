using Lianer.Core.API.DTOs.User;
using Lianer.Core.API.Models;


public interface IUserRepository : ICrud<User>
{
    bool IsEmailTaken(string email, CancellationToken ct);
    Task<IEnumerable<UserSummary>> GetAllUserSummaries(CancellationToken ct);
    Task<UserSummary> GetUserSummaryById(Guid id, CancellationToken ct);
}