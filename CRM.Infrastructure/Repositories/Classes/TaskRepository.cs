using System.Text.Json;
using CRM.Domain.DTOs.Tasks.Requests;
using CRM.Domain.DTOs.Tasks.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_task only — no inline table SQL.</summary>
public sealed class TaskRepository : ITaskRepository
{
    private const string Fn = "public.fn_crm_task";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(ProductDatabaseHelper db, ILogger<TaskRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
        int orgId, int appId, GetTasksRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                status: request.Status,
                priority: request.Priority,
                employeeId: request.EmployeeId,
                limit: request.Limit,
                offset: request.Offset,
                querytype: request.TaskType,
                startDate: request.DueFrom,
                dueDate: request.DueTo);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<TaskListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<TaskDetailDto?> GetTaskByIdAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, taskId: taskId);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<TaskDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<TaskDetailDto> CreateTaskAsync(
        int orgId, int appId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                status: request.Status,
                priority: request.Priority,
                employeeId: request.EmployeeId,
                title: request.Title,
                dueDate: request.DueDate,
                startDate: request.StartDate,
                querytype: request.TaskType,
                notes: request.Notes);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create task returned empty response.");
            return JsonSerializer.Deserialize<TaskDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create task response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<TaskDetailDto> UpdateTaskAsync(
        int orgId, int appId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update", orgId, appId,
                taskId: taskId,
                status: request.Status,
                priority: request.Priority,
                employeeId: request.EmployeeId,
                title: request.Title,
                dueDate: request.DueDate,
                startDate: request.StartDate,
                querytype: request.TaskType,
                notes: request.Notes);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update task returned empty response.");
            return JsonSerializer.Deserialize<TaskDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update task response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update", Fn);
            throw;
        }
    }

    public async Task<TaskDetailDto> CompleteTaskAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("complete", orgId, appId, taskId: taskId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Complete task returned empty response.");
            return JsonSerializer.Deserialize<TaskDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Complete task response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} complete", Fn);
            throw;
        }
    }

    public async Task<DeactivateTaskResponse> DeactivateTaskAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, taskId: taskId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate task returned empty response.");
            return JsonSerializer.Deserialize<DeactivateTaskResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate task response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate", Fn);
            throw;
        }
    }

    public async Task<TaskLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new TaskLookupsResponse();
            return JsonSerializer.Deserialize<TaskLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new TaskLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups", Fn);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? taskId = null,
        string? search = null,
        string? status = null,
        string? priority = null,
        int? employeeId = null,
        int? limit = null,
        int? offset = null,
        string? title = null,
        DateOnly? dueDate = null,
        DateOnly? startDate = null,
        string? querytype = null,
        string? notes = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(taskId),
            Varchar(search),
            Varchar(status),
            Varchar(priority),
            NullableInt(employeeId),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(title),
            Date(dueDate),
            Date(startDate),
            Varchar(querytype),
            Text(notes));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Text(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Text
        };

    private static NpgsqlParameter Date(DateOnly? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
