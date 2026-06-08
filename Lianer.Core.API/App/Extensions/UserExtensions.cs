using Lianer.Core.API.Models;
using Lianer.Core.API.DTOs.User;

public static class UserExtensions
{
    /// <summary>
    /// Projects a User queryable to a UserSummary DTO
    /// </summary>
    public static IQueryable<UserSummary> ProjectToSummary(this IQueryable<User> query)
    {
        return query.Select(u => new UserSummary
        (
            u.Id,         // 1. UserId (Guid)
            u.FullName,   // 2. FullName (string)
            u.Email       // 3. Email (string)
        ));
    }
}