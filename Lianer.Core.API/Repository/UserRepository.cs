using Lianer.Core.API.Common;
using Lianer.Core.API.Data;
using Lianer.Core.API.Models;


public class UserRepository(AppDbContext db) : ACrud<User>(db), IUserRepository
{
    public bool IsEmailTaken(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<UserSummary> GetUserSummaryById(Guid id)
    {
        throw new NotImplementedException();
    }
}