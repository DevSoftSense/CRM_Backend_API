using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Leads.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

/// <summary>
/// Leads API — org_id from JWT, app_id from CRM config (28). Never from browser body.
/// </summary>
[Authorize]
[ApiController]
[Route("api/leads")]
public sealed class LeadController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ILogger<LeadController> _logger;

    public LeadController(ILeadService leadService, ILogger<LeadController> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    /// <summary>GET api/leads?search=&amp;statusId=&amp;sourceId=&amp;ratingId=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetLeadsAsync(
        [FromQuery] string? search,
        [FromQuery] int? statusId,
        [FromQuery] int? sourceId,
        [FromQuery] int? ratingId,
        [FromQuery] int? ownerEmployeeId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.GetLeadsAsync(
                orgId,
                new GetLeadsRequest
                {
                    Search = search,
                    StatusId = statusId,
                    SourceId = sourceId,
                    RatingId = ratingId,
                    OwnerEmployeeId = ownerEmployeeId,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead list failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/leads");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load leads." });
        }
    }

    /// <summary>GET api/leads/lookups — source / status / rating for this org + CRM app</summary>
    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.GetLookupsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead lookups failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/leads/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load lead lookups." });
        }
    }

    /// <summary>GET api/leads/{leadId}</summary>
    [HttpGet("{leadId:long}")]
    public async Task<IActionResult> GetLeadByIdAsync(long leadId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.GetLeadByIdAsync(orgId, leadId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Lead not found." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead get_by_id failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/leads/{LeadId}", leadId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load lead." });
        }
    }

    /// <summary>POST api/leads</summary>
    [HttpPost]
    public async Task<IActionResult> CreateLeadAsync(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.CreateLeadAsync(orgId, userId, request, cancellationToken);
            // Use explicit URI — CreatedAtAction throws "No route matches the supplied values"
            // when link generation fails (shows up in SoftOnCloudSessionMiddleware._next).
            return Created($"/api/leads/{result.LeadId}", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead create failed in DB function.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/leads");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create lead." });
        }
    }

    /// <summary>PUT api/leads/{leadId}</summary>
    [HttpPut("{leadId:long}")]
    public async Task<IActionResult> UpdateLeadAsync(
        long leadId,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.UpdateLeadAsync(orgId, leadId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead update failed in DB function.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/leads/{LeadId}", leadId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update lead." });
        }
    }

    /// <summary>DELETE api/leads/{leadId} — soft delete</summary>
    [HttpDelete("{leadId:long}")]
    public async Task<IActionResult> DeactivateLeadAsync(long leadId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _leadService.DeactivateLeadAsync(orgId, leadId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Lead deactivate failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/leads/{LeadId}", leadId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete lead." });
        }
    }

    private bool TryGetAuthContext(out int userId, out int orgId)
    {
        userId = 0;
        orgId = 0;

        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;

        return int.TryParse(userIdRaw, out userId)
               && userId > 0
               && int.TryParse(orgIdRaw, out orgId)
               && orgId > 0;
    }

    private static string FriendlyDbMessage(PostgresException ex)
    {
        var constraint = ex.ConstraintName ?? string.Empty;
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (constraint.Contains("email", StringComparison.OrdinalIgnoreCase))
                return "A lead with this email already exists for this organisation.";
            if (constraint.Contains("mobile", StringComparison.OrdinalIgnoreCase))
                return "A lead with this mobile already exists for this organisation.";
            if (constraint.Contains("phone", StringComparison.OrdinalIgnoreCase))
                return "A lead with this phone already exists for this organisation.";
            return "This lead already exists (duplicate mobile, phone, or email).";
        }

        return string.IsNullOrWhiteSpace(ex.MessageText) ? "Lead save failed." : ex.MessageText;
    }
}
