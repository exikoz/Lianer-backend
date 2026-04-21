using Lianer.Core.API.Models;

public static class UserExtensions
{
    public static IQueryable<UserSummary> ProjectToSummary
    (this IQueryable<User> query)
    {
        return query.Select(u => new UserSummary
        (
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.CreatedAt,
                u.IsActive,
                u.Provider
            )
        );
    }
}
