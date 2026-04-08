using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Models;

namespace Lianer.Core.API.Repositories;
 
public class UserRepository : IUserRepository<User>
{
    public Task<User> Create(User request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllByQuery(string query)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetById(Guid id)
    {
        throw new NotImplementedException();
    }


    //TODO gör false
    public bool IsEmailTaken(string email)
    {
        return false;
    }

    public Task<User> Update(User request)
    {
        throw new NotImplementedException();
    }
}
