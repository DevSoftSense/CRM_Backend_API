using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Owners.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/owners")]
public sealed class OwnerController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    private readonly ILogger<OwnerController> _logger;

    public OwnerController(IOwnerService ownerService, ILogger<OwnerController> logger)
    {
        _ownerService = ownerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOwnersAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _ownerService.GetOwnersAsync(
                orgId,
                new GetOwnersRequest { Search = search, Status = status, Limit = limit, Offset = offset },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/owners failed");
            return StatusCode(500, new { message = "Failed to load owners." });
        }
    }

    [HttpGet("dropdown", Order = 0)]
    public async Task<IActionResult> GetDropdownAsync(
        [FromQuery] string? search,
        [FromQuery] int limit = 200,
        [FromQuery] bool ensureSeed = true,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _ownerService.GetDropdownAsync(
                orgId, search, limit, ensureSeed, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner dropdown failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/owners/dropdown failed");
            return StatusCode(500, new { message = "Failed to load owners." });
        }
    }

    [HttpGet("{ownerId:int}")]
    public async Task<IActionResult> GetByIdAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _ownerService.GetOwnerByIdAsync(orgId, ownerId, cancellationToken);
            if (result is null) return NotFound(new { message = "Owner not found." });
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner get_by_id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/owners/{OwnerId} failed", ownerId);
            return StatusCode(500, new { message = "Failed to load owner." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _ownerService.CreateOwnerAsync(orgId, userId, request, cancellationToken);
            return Created($"/api/owners/{result.OwnerId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner create failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/owners failed");
            return StatusCode(500, new { message = "Failed to create owner." });
        }
    }

    [HttpPut("{ownerId:int}")]
    public async Task<IActionResult> UpdateAsync(
        int ownerId, [FromBody] UpdateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _ownerService.UpdateOwnerAsync(orgId, ownerId, userId, request, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner update failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/owners/{OwnerId} failed", ownerId);
            return StatusCode(500, new { message = "Failed to update owner." });
        }
    }

    [HttpDelete("{ownerId:int}")]
    public async Task<IActionResult> DeactivateAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _ownerService.DeactivateOwnerAsync(orgId, ownerId, userId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Owner deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/owners/{OwnerId} failed", ownerId);
            return StatusCode(500, new { message = "Failed to delete owner." });
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

    /// <summary>Map unique-constraint failures to clear UI text (fn already raises most cases).</summary>
    private static string FriendlyDbMessage(PostgresException ex)
    {
        var constraint = ex.ConstraintName ?? string.Empty;
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (constraint.Contains("email", StringComparison.OrdinalIgnoreCase))
                return "An owner with this email already exists.";
            if (constraint.Contains("phone", StringComparison.OrdinalIgnoreCase)
                || constraint.Contains("mobile", StringComparison.OrdinalIgnoreCase))
                return "An owner with this mobile number already exists.";
            if (constraint.Contains("code", StringComparison.OrdinalIgnoreCase)
                || constraint.Contains("employee_code", StringComparison.OrdinalIgnoreCase))
                return "An owner with this code already exists.";
            return "This owner already exists (duplicate mobile, email, or code).";
        }

        return string.IsNullOrWhiteSpace(ex.MessageText) ? "Owner save failed." : ex.MessageText;
    }
}
