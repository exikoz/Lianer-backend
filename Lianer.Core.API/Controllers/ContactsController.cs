using Asp.Versioning;
using Lianer.Core.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Lianer.Core.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contacts")]
[Produces("application/json")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _service;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactService service, ILogger<ContactsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get contact by id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> GetById(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("GET contact {Id}", id);

        var contact = await _service.GetContactById(id, ct);

        if (contact is null)
            return NotFound();

        return Ok(contact);
    }

    /// <summary>
    /// Create new contact
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateContactRequest request, CancellationToken ct)
    {
        _logger.LogInformation("POST create contact");

        var id = await _service.Create(request, ct);

        return CreatedAtAction(nameof(GetById),
            new { version = "1.0", id },
            id);
    }

    /// <summary>
    /// Update existing contact
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken ct)
    {
        _logger.LogInformation("PUT update contact {Id}", id);


        var updatedId = await _service.Update(id,request, ct);

        return Ok(updatedId);
    }

    /// <summary>
    /// Delete contact
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("DELETE contact {Id}", id);

        await _service.Delete(id, ct);

        return NoContent();
    }
}