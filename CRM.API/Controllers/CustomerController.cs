using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Customers.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/customers")]
public sealed class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomersAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? accountTypeId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetCustomersAsync(
                orgId,
                new GetCustomersRequest
                {
                    Search = search,
                    Status = status,
                    AccountTypeId = accountTypeId,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/customers failed");
            return StatusCode(500, new { message = "Failed to load customers." });
        }
    }

    /// <summary>Light list for Opportunity Account dropdown.</summary>
    [HttpGet("dropdown", Order = 0)]
    public async Task<IActionResult> GetDropdownAsync(
        [FromQuery] string? search,
        [FromQuery] int limit = 200,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetDropdownAsync(orgId, search, limit, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer dropdown failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/customers/dropdown failed");
            return StatusCode(500, new { message = "Failed to load customer dropdown." });
        }
    }

    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _customerService.GetLookupsAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer lookups failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/customers/lookups failed");
            return StatusCode(500, new { message = "Failed to load customer lookups." });
        }
    }

    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> GetByIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetCustomerByIdAsync(orgId, accountId, cancellationToken);
            if (result is null) return NotFound(new { message = "Customer not found." });
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer get_by_id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/customers/{AccountId} failed", accountId);
            return StatusCode(500, new { message = "Failed to load customer." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.CreateCustomerAsync(orgId, userId, request, cancellationToken);
            return Created($"/api/customers/{result.AccountId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer create failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/customers failed");
            return StatusCode(500, new { message = "Failed to create customer." });
        }
    }

    [HttpPut("{accountId:int}")]
    public async Task<IActionResult> UpdateAsync(
        int accountId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.UpdateCustomerAsync(orgId, accountId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer update failed.");
            return BadRequest(new { message = FriendlyDbMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/customers/{AccountId} failed", accountId);
            return StatusCode(500, new { message = "Failed to update customer." });
        }
    }

    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> DeactivateAsync(int accountId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _customerService.DeactivateCustomerAsync(orgId, accountId, userId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/customers/{AccountId} failed", accountId);
            return StatusCode(500, new { message = "Failed to delete customer." });
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
                return "A customer with this email already exists for this organisation.";
            if (constraint.Contains("phone", StringComparison.OrdinalIgnoreCase)
                || constraint.Contains("mobile", StringComparison.OrdinalIgnoreCase))
                return "A customer with this mobile already exists for this organisation.";
            return "This customer already exists (duplicate mobile or email).";
        }

        return string.IsNullOrWhiteSpace(ex.MessageText) ? "Customer save failed." : ex.MessageText;
    }
}
