using Lianer.Core.API.Models;

public interface IContactRepository : ICrud<Contact>
{
    Task<IReadOnlyList<Contact>> GetAll(int currentPage,int pageSize, CancellationToken ct);
}