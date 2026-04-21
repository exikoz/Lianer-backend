using Lianer.Core.API.Models;


public interface IUserRepository : ICrud<User>
{
    bool IsEmailTaken(string email);
}