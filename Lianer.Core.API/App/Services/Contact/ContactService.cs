using Lianer.Core.API.Common;
using Lianer.Core.API.DTOs;
using Lianer.Core.API.Models;

public class ContactService(IContactRepository repo) : IContactService
{
    private readonly IContactRepository _repo = repo;

    #region Write operations

    public async Task<Guid> Create(CreateContactRequest request, CancellationToken ct)
    {
        ValidationHelper(request);

        var contact = Contact.Create(
            request.FirstName,
            request.LastName,
            request.Role,
            request.Company,
            request.Phone,
            request.Email,
            request.Social is null
                ? null
                : new ContactSocial
                {
                    LinkedIn = request.Social.LinkedIn,
                    Website = request.Social.Website
                },
            request.Status,
            request.AssignedTo,
            request.IsFavorite,
            null,
            null
        );

        var created = await _repo.Create(contact, ct);
        return created.Id;
    }

    public async Task<Guid> Update(Guid id, UpdateContactRequest request, CancellationToken ct)
    {
        Guard.Against.Null(request);

        var contact = await _repo.GetById(id, ct)
            ?? throw new NotFoundException("Contact with id: {Id} could not be found", id);

        contact.Update(
            request.FirstName ?? contact.FirstName,
            request.LastName ?? contact.LastName,
            request.Role ?? contact.Role,
            request.Company ?? contact.Company,
            request.Phone ?? contact.Phone,
            request.Email ?? contact.Email,
            request.Social is null
                ? null
                : new ContactSocial
                {
                    LinkedIn = request.Social.LinkedIn,
                    Website = request.Social.Website
                },
            request.Status ?? contact.Status,
            request.AssignedTo ?? contact.AssignedTo,
            request.IsFavorite ?? contact.IsFavorite,
            request.CompletedAt ?? contact.CompletedAt,
            request.LastContactDate ?? contact.LastContactDate
        );

        var updated = await _repo.Update(contact, ct);
        return updated.Id;
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(id);
        await _repo.Delete(id, ct);
    }

    #endregion

    #region Read operations

    public async Task<ContactResponse?> GetContactById(Guid id, CancellationToken ct)
    {
        Guard.Against.NullOrEmptyGuid(id);

        var contact = await _repo.GetById(id, ct);

        if (contact is null)
            return null;

        return new ContactResponse
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Role = contact.Role,
            Company = contact.Company,
            Phone = contact.Phone,
            Email = contact.Email,

            Social = new ContactSocialDto
            {
                LinkedIn = contact.Social.LinkedIn,
                Website = contact.Social.Website
            },

            Status = contact.Status,
            AssignedTo = contact.AssignedTo,
            IsFavorite = contact.IsFavorite,
            CompletedAt = contact.CompletedAt,
            LastContactDate = contact.LastContactDate,

            InteractionLog = [.. contact.InteractionLog
                .Select(x => new InteractionLogEntryDto
                {
                    Id = x.Id,
                    Date = x.Date,
                    Type = x.Type,
                    Content = x.Content,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus
                })]
        };
    }

    #endregion

    private static void ValidationHelper(CreateContactRequest request)
    {
        Guard.Against.Null(request);
        Guard.Against.NullOrWhiteSpace(request.FirstName);
        Guard.Against.NullOrWhiteSpace(request.LastName);
    }
}