using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Timeline.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/timeline")]
public sealed class TimelineController : ControllerBase
{
    private readonly ITimelineService _timelineService;
    private readonly ILogger<TimelineController> _logger;

    public TimelineController(ITimelineService timelineService, ILogger<TimelineController> logger)
    {
        _timelineService = timelineService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTimelineAsync(
        [FromQuery] string entityType = "CUSTOMER",
        [FromQuery] long entityId = 0,
        [FromQuery] string interactionType = "ALL",
        [FromQuery] int? ownerId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var request = new GetTimelineRequest
            {
                EntityType = entityType,
                EntityId = entityId,
                InteractionType = interactionType,
                OwnerId = ownerId,
                Search = search,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Limit = limit,
                Offset = offset
            };

            var items = await _timelineService.GetTimelineAsync(orgId, request, cancellationToken);
            return Ok(items);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving timeline");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/timeline failed");
            return StatusCode(500, new { message = "Failed to load timeline events." });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync(
        [FromQuery] long entityId,
        [FromQuery] string entityType = "CUSTOMER",
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (entityId <= 0)
            return BadRequest(new { message = "entityId is required." });

        try
        {
            var summary = await _timelineService.GetSummaryAsync(orgId, entityType, entityId, cancellationToken);
            return Ok(summary);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving summary for entity {EntityId}", entityId);
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/timeline/summary failed for entity {EntityId}", entityId);
            return StatusCode(500, new { message = "Failed to load entity summary." });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(
        [FromQuery] long? entityId = null,
        [FromQuery] string entityType = "CUSTOMER",
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var stats = await _timelineService.GetStatsAsync(orgId, entityType, entityId, cancellationToken);
            return Ok(stats);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving timeline stats");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/timeline/stats failed");
            return StatusCode(500, new { message = "Failed to load timeline statistics." });
        }
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookupsAsync(
        [FromQuery] long? entityId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var lookups = await _timelineService.GetLookupsAsync(orgId, entityId, cancellationToken);
            return Ok(lookups);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving timeline lookups");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/timeline/lookups failed");
            return StatusCode(500, new { message = "Failed to load timeline lookups." });
        }
    }

    [HttpPost("activity")]
    public async Task<IActionResult> LogInteractionAsync(
        [FromBody] LogInteractionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _timelineService.LogInteractionAsync(orgId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error logging interaction");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/timeline/activity failed");
            return StatusCode(500, new { message = "Failed to log interaction." });
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
}

