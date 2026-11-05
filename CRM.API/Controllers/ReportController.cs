using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Reports.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService service, ILogger<ReportController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets the master catalog of all 14 CRM reports and their strategic categories.
    /// </summary>
    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var catalog = await _service.GetCatalogAsync(orgId, cancellationToken);
            return Ok(catalog);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error retrieving Report catalog");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/reports/catalog failed");
            return StatusCode(500, new { message = "Failed to load report catalog." });
        }
    }

    /// <summary>
    /// Runs a specific CRM report with filters via POST body.
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunReportPostAsync(
        [FromBody] RunReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request == null || string.IsNullOrWhiteSpace(request.ReportCode))
            return BadRequest(new { message = "ReportCode is required." });

        try
        {
            var result = await _service.RunReportAsync(orgId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error running report {ReportCode}", request.ReportCode);
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/reports/run failed for {ReportCode}", request.ReportCode);
            return StatusCode(500, new { message = "Failed to run report." });
        }
    }

    /// <summary>
    /// Runs a specific CRM report with filters via GET query parameters.
    /// </summary>
    [HttpGet("run")]
    public async Task<IActionResult> RunReportGetAsync(
        [FromQuery] string reportCode,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? ownerId = null,
        [FromQuery] string? extraFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (string.IsNullOrWhiteSpace(reportCode))
            return BadRequest(new { message = "ReportCode query parameter is required." });

        try
        {
            var request = new RunReportRequest
            {
                ReportCode = reportCode.Trim(),
                DateFrom = dateFrom,
                DateTo = dateTo,
                OwnerEmployeeId = ownerId,
                ExtraFilter = extraFilter
            };

            var result = await _service.RunReportAsync(orgId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Database error running report {ReportCode}", reportCode);
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/reports/run failed for {ReportCode}", reportCode);
            return StatusCode(500, new { message = "Failed to run report." });
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

