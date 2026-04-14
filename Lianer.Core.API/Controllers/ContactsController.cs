using Asp.Versioning;
using Lianer.Core.API.DTOs;
using Lianer.Core.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

/// <summary>
/// Manages CRM contact resources.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactsController : ControllerBase
{
    private static readonly List<ContactItem> _contacts = [];
    private static int _nextId = 1;

    /// <summary>
    /// Gets all contacts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContactResponse>), StatusCodes.Status200OK)]
    public ActionResult<List<ContactResponse>> GetAll()
    {
        var response = _contacts.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Gets a contact by ID.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ContactResponse> GetById(int id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact is null)
            return NotFound();

        return Ok(MapToResponse(contact));
    }

    /// <summary>
    /// Creates a new contact.
    /// </summary>
    /// <param name="request">The contact data.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<ContactResponse> Create([FromBody] CreateContactRequest request)
    {
        // Duplicate name check (case-insensitive), matching frontend behavior
        var duplicate = _contacts.FirstOrDefault(c =>
            c.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (duplicate is not null)
        {
            ModelState.AddModelError("Name", $"A contact with the name \"{request.Name}\" already exists.");
            return ValidationProblem();
        }

        var contact = new ContactItem
        {
            Id = _nextId++,
            Name = request.Name.Trim(),
            Role = request.Role.Trim(),
            Company = request.Company.Trim(),
            Phone = request.Phone,
            Email = request.Email,
            Social = new ContactSocial
            {
                LinkedIn = request.Social?.LinkedIn,
                Website = request.Social?.Website
            },
            Status = request.Status,
            AssignedTo = request.AssignedTo,
            IsFavorite = request.IsFavorite,
            CompletedAt = request.Status == "Klar" ? DateTime.UtcNow : null
        };

        _contacts.Add(contact);

        var response = MapToResponse(contact);
        return CreatedAtAction(nameof(GetById), new { version = "1.0", id = contact.Id }, response);
    }

    /// <summary>
    /// Updates an existing contact.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    /// <param name="request">The updated contact data.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ContactResponse> Update(int id, [FromBody] UpdateContactRequest request)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact is null)
            return NotFound();

        // Duplicate name check excluding current contact
        var duplicate = _contacts.FirstOrDefault(c =>
            c.Id != id && c.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (duplicate is not null)
        {
            ModelState.AddModelError("Name", $"A contact with the name \"{request.Name}\" already exists.");
            return ValidationProblem();
        }

        var wasKlar = contact.Status == "Klar";

        contact.Name = request.Name.Trim();
        contact.Role = request.Role.Trim();
        contact.Company = request.Company.Trim();
        contact.Phone = request.Phone;
        contact.Email = request.Email;
        contact.Social = new ContactSocial
        {
            LinkedIn = request.Social?.LinkedIn,
            Website = request.Social?.Website
        };
        contact.Status = request.Status;
        contact.AssignedTo = request.AssignedTo;
        contact.IsFavorite = request.IsFavorite;

        // Set CompletedAt when status transitions to "Klar"
        if (request.Status == "Klar" && !wasKlar)
            contact.CompletedAt = DateTime.UtcNow;
        else if (request.Status != "Klar")
            contact.CompletedAt = null;

        return Ok(MapToResponse(contact));
    }

    /// <summary>
    /// Deletes a contact by ID.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact is null)
            return NotFound();

        _contacts.Remove(contact);
        return NoContent();
    }

    private static ContactResponse MapToResponse(ContactItem c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Role = c.Role,
        Company = c.Company,
        Phone = c.Phone,
        Email = c.Email,
        Social = new ContactSocialDto
        {
            LinkedIn = c.Social.LinkedIn,
            Website = c.Social.Website
        },
        Status = c.Status,
        AssignedTo = c.AssignedTo,
        IsFavorite = c.IsFavorite,
        CompletedAt = c.CompletedAt
    };
}
