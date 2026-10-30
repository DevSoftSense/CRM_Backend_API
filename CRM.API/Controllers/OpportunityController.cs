using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Opportunities.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

/// <summary>
/// Opportunities API — org_id from JWT, app_id from CRM config (28). Never from browser body.
/// </summary>
[Authorize]
[ApiController]
[Route("api/opportunities")]
public sealed class OpportunityController : ControllerBase
{
    private readonly IOpportunityService _opportunityService;
    private readonly ILogger<OpportunityController> _logger;

    public OpportunityController(IOpportunityService opportunityService, ILogger<OpportunityController> logger)
    {
        _opportunityService = opportunityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOpportunitiesAsync(
        [FromQuery] string? search,
        [FromQuery] int? stageId,
        [FromQuery] int? opportunityTypeId,
        [FromQuery] int? sourceId,
        [FromQuery] int? priorityId,
        [FromQuery] int? ownerEmployeeId,
        [FromQuery] int? accountId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.GetOpportunitiesAsync(
                orgId,
                new GetOpportunitiesRequest
                {
                    Search = search,
                    StageId = stageId,
                    OpportunityTypeId = opportunityTypeId,
                    SourceId = sourceId,
                    PriorityId = priorityId,
                    OwnerEmployeeId = ownerEmployeeId,
                    AccountId = accountId,
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
            _logger.LogWarning(ex, "Opportunity list failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/opportunities");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load opportunities." });
        }
    }

    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.GetLookupsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Opportunity lookups failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/opportunities/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load opportunity lookups." });
        }
    }

    [HttpGet("pipeline", Order = 0)]
    public async Task<IActionResult> GetPipelineAsync(
        [FromQuery] string? search,
        [FromQuery] int? ownerEmployeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _opportunityService.GetPipelineAsync(orgId, search, ownerEmployeeId, fromDate, toDate, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Pipeline load failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/opportunities/pipeline failed");
            return StatusCode(500, new { message = "Failed to load pipeline." });
        }
    }

    [HttpPut("{opportunityId:long}/stage", Order = 0)]
    public async Task<IActionResult> MoveStageAsync(
        long opportunityId,
        [FromBody] MoveOpportunityStageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.MoveStageAsync(
                orgId, opportunityId, request.StageId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Move stage failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/opportunities/{OpportunityId}/stage failed", opportunityId);
            return StatusCode(500, new { message = "Failed to move opportunity stage." });
        }
    }

    [HttpGet("{opportunityId:long}")]
    public async Task<IActionResult> GetOpportunityByIdAsync(long opportunityId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.GetOpportunityByIdAsync(orgId, opportunityId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Opportunity not found." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Opportunity get_by_id failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/opportunities/{OpportunityId}", opportunityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load opportunity." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOpportunityAsync(
        [FromBody] CreateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.CreateOpportunityAsync(orgId, userId, request, cancellationToken);
            return Created($"/api/opportunities/{result.OpportunityId}", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Opportunity create failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/opportunities");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create opportunity." });
        }
    }

    [HttpPut("{opportunityId:long}")]
    public async Task<IActionResult> UpdateOpportunityAsync(
        long opportunityId,
        [FromBody] UpdateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.UpdateOpportunityAsync(
                orgId, opportunityId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Opportunity update failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/opportunities/{OpportunityId}", opportunityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update opportunity." });
        }
    }

    [HttpDelete("{opportunityId:long}")]
    public async Task<IActionResult> DeactivateOpportunityAsync(
        long opportunityId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _opportunityService.DeactivateOpportunityAsync(
                orgId, opportunityId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Opportunity deactivate failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/opportunities/{OpportunityId}", opportunityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete opportunity." });
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
}
