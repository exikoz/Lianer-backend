using Lianer.Core.API.Common;
using Lianer.Core.API.Data;
using Lianer.Core.API.Models;
using Lianer.Core.API.DTOs.User;
using Microsoft.EntityFrameworkCore;


/// <summary>
/// Repository for user-specific data operations
/// </summary>
public class UserRepository(AppDbContext db) : ACrud<User>(db), IUserRepository
{
    /// <summary>
    /// Checks if an email address is already associated with an account
    /// </summary>
    public bool IsEmailTaken(string email, CancellationToken ct)
    {
        // Check for any existing users with the same email address
        return _db.Users.Any(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves a summary of a user's basic information by their ID
    /// </summary>
    public async Task<UserSummary> GetUserSummaryById(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null!;

        // Use constructor syntax for the positional record
        return new UserSummary(user.Id, user.FullName, user.Email);
    }

    /// <summary>
    /// Retrieves summaries for all users in the database
    /// </summary>
    public async Task<IEnumerable<UserSummary>> GetAllUserSummaries(CancellationToken ct)
    {
        return await _db.Users
            .Select(u => new UserSummary(u.Id, u.FullName, u.Email))
            .ToListAsync();
    }
}