using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Tasks.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public sealed class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(ITaskService taskService, ILogger<TaskController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? taskType,
        [FromQuery] int? employeeId,
        [FromQuery] DateOnly? dueFrom,
        [FromQuery] DateOnly? dueTo,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _taskService.GetTasksAsync(
                orgId,
                new GetTasksRequest
                {
                    Search = search,
                    Status = status,
                    Priority = priority,
                    TaskType = taskType,
                    EmployeeId = employeeId,
                    DueFrom = dueFrom,
                    DueTo = dueTo,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/tasks failed");
            return StatusCode(500, new { message = "Failed to load tasks." });
        }
    }

    [HttpGet("lookups", Order = 0)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _taskService.GetLookupsAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task lookups failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/tasks/lookups failed");
            return StatusCode(500, new { message = "Failed to load task lookups." });
        }
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _taskService.GetTaskByIdAsync(orgId, taskId, cancellationToken);
            if (result is null) return NotFound(new { message = "Task not found." });
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task get_by_id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/tasks/{TaskId} failed", taskId);
            return StatusCode(500, new { message = "Failed to load task." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _taskService.CreateTaskAsync(orgId, request, cancellationToken);
            return Created($"/api/tasks/{result.TaskId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task create failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/tasks failed");
            return StatusCode(500, new { message = "Failed to create task." });
        }
    }

    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> UpdateAsync(
        int taskId, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _taskService.UpdateTaskAsync(orgId, taskId, request, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task update failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT api/tasks/{TaskId} failed", taskId);
            return StatusCode(500, new { message = "Failed to update task." });
        }
    }

    [HttpPost("{taskId:int}/complete", Order = 0)]
    public async Task<IActionResult> CompleteAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _taskService.CompleteTaskAsync(orgId, taskId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task complete failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/tasks/{TaskId}/complete failed", taskId);
            return StatusCode(500, new { message = "Failed to complete task." });
        }
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeactivateAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _taskService.DeactivateTaskAsync(orgId, taskId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Task deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/tasks/{TaskId} failed", taskId);
            return StatusCode(500, new { message = "Failed to delete task." });
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
