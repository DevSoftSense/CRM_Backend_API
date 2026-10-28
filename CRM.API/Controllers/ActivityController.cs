using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Activities.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/activities")]
public sealed class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(IActivityService activityService, ILogger<ActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivitiesAsync(
        [FromQuery] string? search,
        [FromQuery] string? actType,
        [FromQuery] string? activityStatus,
        [FromQuery] int? employeeId,
        [FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? startTo,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _activityService.GetActivitiesAsync(
                orgId,
                new GetActivitiesRequest
                {
                    Search = search,
                    ActType = actType,
                    ActivityStatus = activityStatus,
                    EmployeeId = employeeId,
                    StartFrom = startFrom,
                    StartTo = startTo,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/activities failed");
            return StatusCode(500, new { message = "Failed to load activities." });
        }
    }

    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _activityService.GetLookupsAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity lookups failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/activities/lookups failed");
            return StatusCode(500, new { message = "Failed to load activity lookups." });
        }
    }

    [HttpGet("calendar", Order = 0)]
    public async Task<IActionResult> GetCalendarAsync(
        [FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? startTo,
        [FromQuery] int? employeeId,
        [FromQuery] string? actType,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _activityService.GetCalendarAsync(
                orgId,
                new GetActivitiesRequest
                {
                    StartFrom = startFrom,
                    StartTo = startTo,
                    EmployeeId = employeeId,
                    ActType = actType,
                    Limit = 2000
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity calendar failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/activities/calendar failed");
            return StatusCode(500, new { message = "Failed to load calendar." });
        }
    }

    [HttpGet("{activityId:int}")]
    public async Task<IActionResult> GetByIdAsync(int activityId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _activityService.GetActivityByIdAsync(orgId, activityId, cancellationToken);
            if (result is null) return NotFound(new { message = "Activity not found." });
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity get_by_id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/activities/{ActivityId} failed", activityId);
            return StatusCode(500, new { message = "Failed to load activity." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _activityService.CreateActivityAsync(orgId, request, cancellationToken);
            return Created($"/api/activities/{result.ActivityId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity create failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/activities failed");
            return StatusCode(500, new { message = "Failed to create activity." });
        }
    }

    [HttpPut("{activityId:int}")]
    public async Task<IActionResult> UpdateAsync(
        int activityId, [FromBody] UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _activityService.UpdateActivityAsync(orgId, activityId, request, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity update failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/activities/{ActivityId} failed", activityId);
            return StatusCode(500, new { message = "Failed to update activity." });
        }
    }

    [HttpPost("{activityId:int}/complete", Order = 0)]
    public async Task<IActionResult> CompleteAsync(int activityId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _activityService.CompleteActivityAsync(orgId, activityId, null, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity complete failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/activities/{ActivityId}/complete failed", activityId);
            return StatusCode(500, new { message = "Failed to complete activity." });
        }
    }

    [HttpDelete("{activityId:int}")]
    public async Task<IActionResult> DeactivateAsync(int activityId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _activityService.DeactivateActivityAsync(orgId, activityId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Activity deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/activities/{ActivityId} failed", activityId);
            return StatusCode(500, new { message = "Failed to delete activity." });
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
