using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.SalesDashboard.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sales-dashboard")]
public sealed class SalesDashboardController : ControllerBase
{
    private readonly ISalesDashboardService _service;
    private readonly ILogger<SalesDashboardController> _logger;

    public SalesDashboardController(ISalesDashboardService service, ILogger<SalesDashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesDashboardAsync(
        [FromQuery] string period = "this_month",
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var request = new GetSalesDashboardRequest
            {
                Period = string.IsNullOrWhiteSpace(period) ? "this_month" : period,
                DateFrom = dateFrom,
                DateTo = dateTo,
                OwnerEmployeeId = ownerId
            };

            var data = await _service.GetSalesDashboardAsync(orgId, request, cancellationToken);
            return Ok(data);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving Sales Dashboard");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/sales-dashboard failed");
            return StatusCode(500, new { message = "Failed to load Sales Dashboard." });
        }
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var lookups = await _service.GetLookupsAsync(orgId, cancellationToken);
            return Ok(lookups);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving Sales Dashboard lookups");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/sales-dashboard/lookups failed");
            return StatusCode(500, new { message = "Failed to load Sales Dashboard lookups." });
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

