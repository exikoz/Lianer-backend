using Lianer.Core.API.Data;
using Lianer.Core.API.Models;
using Microsoft.EntityFrameworkCore;

public class ContactRepository(AppDbContext db) : ACrud<Contact>(db), IContactRepository
{
    
    public async Task<IReadOnlyList<Contact>> GetAll(
        int currentPage,
        int pageSize,
        CancellationToken ct)
    {
        var skip = (currentPage - 1) * pageSize;

        return await _db.Contacts
            .AsNoTracking()
            .Include(x => x.Social)
            .Include(x => x.InteractionLog)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}

