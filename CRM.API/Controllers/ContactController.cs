using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Contacts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/contacts")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetContactsAsync(
        [FromQuery] string? search,
        [FromQuery] int? accountId,
        [FromQuery] bool? isPrimary,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _contactService.GetContactsAsync(
                orgId,
                new GetContactsRequest
                {
                    Search = search,
                    AccountId = accountId,
                    IsPrimary = isPrimary,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/contacts failed");
            return StatusCode(500, new { message = "Failed to load contacts." });
        }
    }

    /// <summary>Light list for Opportunity Primary Contact dropdown (filter by accountId).</summary>
    [HttpGet("dropdown", Order = 0)]
    public async Task<IActionResult> GetDropdownAsync(
        [FromQuery] int? accountId,
        [FromQuery] string? search,
        [FromQuery] int limit = 200,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _contactService.GetDropdownAsync(orgId, accountId, search, limit, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact dropdown failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/contacts/dropdown failed");
            return StatusCode(500, new { message = "Failed to load contact dropdown." });
        }
    }

    [HttpGet("{contactPersonId:int}")]
    public async Task<IActionResult> GetByIdAsync(int contactPersonId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _contactService.GetContactByIdAsync(orgId, contactPersonId, cancellationToken);
            if (result is null) return NotFound(new { message = "Contact not found." });
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact get_by_id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/contacts/{ContactPersonId} failed", contactPersonId);
            return StatusCode(500, new { message = "Failed to load contact." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _contactService.CreateContactAsync(orgId, request, cancellationToken);
            return Created($"/api/contacts/{result.ContactPersonId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact create failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/contacts failed");
            return StatusCode(500, new { message = "Failed to create contact." });
        }
    }

    [HttpPut("{contactPersonId:int}")]
    public async Task<IActionResult> UpdateAsync(
        int contactPersonId, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _contactService.UpdateContactAsync(orgId, contactPersonId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact update failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/contacts/{ContactPersonId} failed", contactPersonId);
            return StatusCode(500, new { message = "Failed to update contact." });
        }
    }

    [HttpDelete("{contactPersonId:int}")]
    public async Task<IActionResult> DeactivateAsync(int contactPersonId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _contactService.DeactivateContactAsync(orgId, contactPersonId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Contact deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/contacts/{ContactPersonId} failed", contactPersonId);
            return StatusCode(500, new { message = "Failed to delete contact." });
        }
    }

    private bool TryGetAuthContext(out int userId, out int orgId)
    {
        userId = 0;
        orgId = 0;
        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;
        return int.TryParse(userIdRaw, out userId) && userId > 0
               && int.TryParse(orgIdRaw, out orgId) && orgId > 0;
    }

    private static string FriendlyDbMessage(PostgresException ex)
    {
        var constraint = ex.ConstraintName ?? string.Empty;
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (constraint.Contains("email", StringComparison.OrdinalIgnoreCase))
                return "A contact with this email already exists for this organisation.";
            if (constraint.Contains("phone", StringComparison.OrdinalIgnoreCase)
                || constraint.Contains("mobile", StringComparison.OrdinalIgnoreCase))
                return "A contact with this mobile already exists for this organisation.";
            return "This contact already exists (duplicate mobile or email).";
        }

        return string.IsNullOrWhiteSpace(ex.MessageText) ? "Contact save failed." : ex.MessageText;
    }
}
