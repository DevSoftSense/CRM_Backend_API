using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Campaigns.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

/// <summary>
/// Campaigns API — Multi-tenant via JWT (org_id, user_id) and CRM config (app_id=28).
/// Zero inline SQL — all persistence delegates to public.fn_crm_campaign.
/// </summary>
[Authorize]
[ApiController]
[Route("api/campaigns")]
public sealed class CampaignController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(ICampaignService campaignService, ILogger<CampaignController> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    /// <summary>GET api/campaigns — Paginated and filtered campaigns list</summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignsAsync(
        [FromQuery] string? search,
        [FromQuery] int? channelId,
        [FromQuery] int? statusId,
        [FromQuery] int? ownerEmployeeId,
        [FromQuery] DateOnly? startFrom,
        [FromQuery] DateOnly? startTo,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.GetCampaignsAsync(
                orgId,
                new GetCampaignsRequest
                {
                    Search = search,
                    ChannelId = channelId,
                    StatusId = statusId,
                    OwnerEmployeeId = ownerEmployeeId,
                    StartFrom = startFrom,
                    StartTo = startTo,
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
            _logger.LogWarning(ex, "Campaign list failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/campaigns");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load campaigns." });
        }
    }

    /// <summary>GET api/campaigns/lookups — Channels, statuses, owners, and currencies</summary>
    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.GetLookupsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign lookups failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/campaigns/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load campaign lookups." });
        }
    }

    /// <summary>GET api/campaigns/dropdown — Lightweight campaign picker for forms</summary>
    [HttpGet("dropdown", Order = 0)]
    public async Task<IActionResult> GetDropdownAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.GetDropdownAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign dropdown failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/campaigns/dropdown");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load campaign dropdown." });
        }
    }

    /// <summary>GET api/campaigns/stats — Executive KPI metrics and channel breakdown</summary>
    [HttpGet("stats", Order = 0)]
    public async Task<IActionResult> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.GetStatsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign stats failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/campaigns/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load campaign stats." });
        }
    }

    /// <summary>GET api/campaigns/{id} — Detailed campaign with linked leads and opportunities</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCampaignByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.GetCampaignByIdAsync(orgId, id, cancellationToken);
            if (result == null)
                return NotFound(new { message = $"Campaign with ID {id} was not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get campaign by id failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/campaigns/{CampaignId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load campaign." });
        }
    }

    /// <summary>POST api/campaigns — Create a new campaign</summary>
    [HttpPost]
    public async Task<IActionResult> CreateCampaignAsync(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var created = await _campaignService.CreateCampaignAsync(orgId, userId, request, cancellationToken);
            return Created($"/api/campaigns/{created.CampaignId}", created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign create failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/campaigns");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create campaign." });
        }
    }

    /// <summary>PUT api/campaigns/{id} — Update an existing campaign</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateCampaignAsync(
        long id,
        [FromBody] UpdateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var updated = await _campaignService.UpdateCampaignAsync(orgId, id, userId, request, cancellationToken);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign update failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/campaigns/{CampaignId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update campaign." });
        }
    }

    /// <summary>DELETE api/campaigns/{id} — Deactivate (soft-delete) a campaign</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeactivateCampaignAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _campaignService.DeactivateCampaignAsync(orgId, id, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Campaign deactivate failed in DB function.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/campaigns/{CampaignId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete campaign." });
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

