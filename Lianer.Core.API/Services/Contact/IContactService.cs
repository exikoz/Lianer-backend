using Lianer.Core.API.DTOs;

public interface IContactService
{
    Task<Guid> Create(CreateContactRequest request, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
    Task<ContactResponse?> GetContactById(Guid id, CancellationToken ct);
    Task<Guid> Update(Guid id,UpdateContactRequest request, CancellationToken ct);
}
