public interface IUserRepository<User> : ICrudRepository<User>
{
    bool IsEmailTaken(string email);
}