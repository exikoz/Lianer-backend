using System.Linq.Expressions;
using Lianer.Core.API.Projection;

namespace Lianer.Core.API.Models;

public record ContactSummary
(
    Guid Id,
    string FirstName,
    string LastName,
    string Company,
    List<string> Phone,
    List<string> Email
) : IProjection<Contact, ContactSummary>
{
    public static Expression<Func<Contact, ContactSummary>> Selector => 
    c => new ContactSummary(c.Id, c.FirstName, c.LastName,c.Company, c.Phone, c.Email);
}