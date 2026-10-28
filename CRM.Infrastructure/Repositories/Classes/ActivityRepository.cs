using System.Text.Json;
using CRM.Domain.DTOs.Activities.Requests;
using CRM.Domain.DTOs.Activities.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_activity only — no inline table SQL.</summary>
public sealed class ActivityRepository : IActivityRepository
{
    private const string Fn = "public.fn_crm_activity";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ActivityRepository> _logger;

    public ActivityRepository(ProductDatabaseHelper db, ILogger<ActivityRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task<IReadOnlyList<ActivityListItemDto>> GetActivitiesAsync(
        int orgId, int appId, GetActivitiesRequest request, CancellationToken cancellationToken = default) =>
        QueryListAsync("list", orgId, appId, request, cancellationToken);

    public Task<IReadOnlyList<ActivityListItemDto>> GetCalendarAsync(
        int orgId, int appId, GetActivitiesRequest request, CancellationToken cancellationToken = default) =>
        QueryListAsync("calendar", orgId, appId, request, cancellationToken);

    public async Task<ActivityDetailDto?> GetActivityByIdAsync(
        int orgId, int appId, int activityId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, activityId: activityId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;
            return JsonSerializer.Deserialize<ActivityDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<ActivityDetailDto> CreateActivityAsync(
        int orgId, int appId, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                actType: request.ActType,
                employeeId: request.EmployeeId,
                subject: request.Subject,
                startTime: request.StartTime,
                endTime: request.EndTime,
                callNumber: request.CallNumber,
                callNotes: request.CallNotes,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create activity returned empty response.");

            return JsonSerializer.Deserialize<ActivityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create activity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<ActivityDetailDto> UpdateActivityAsync(
        int orgId, int appId, int activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update", orgId, appId,
                activityId: activityId,
                actType: request.ActType,
                employeeId: request.EmployeeId,
                subject: request.Subject,
                startTime: request.StartTime,
                endTime: request.EndTime,
                callNumber: request.CallNumber,
                callNotes: request.CallNotes,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update activity returned empty response.");

            return JsonSerializer.Deserialize<ActivityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update activity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update", Fn);
            throw;
        }
    }

    public async Task<ActivityDetailDto> CompleteActivityAsync(
        int orgId, int appId, int activityId, DateTime? endTime, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("complete", orgId, appId, activityId: activityId, endTime: endTime);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Complete activity returned empty response.");

            return JsonSerializer.Deserialize<ActivityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Complete activity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} complete", Fn);
            throw;
        }
    }

    public async Task<DeactivateActivityResponse> DeactivateActivityAsync(
        int orgId, int appId, int activityId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, activityId: activityId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate activity returned empty response.");

            return JsonSerializer.Deserialize<DeactivateActivityResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate activity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate", Fn);
            throw;
        }
    }

    public async Task<ActivityLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new ActivityLookupsResponse();

            return JsonSerializer.Deserialize<ActivityLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new ActivityLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups", Fn);
            throw;
        }
    }

    private async Task<IReadOnlyList<ActivityListItemDto>> QueryListAsync(
        string action, int orgId, int appId, GetActivitiesRequest request, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                action, orgId, appId,
                search: request.Search,
                actType: request.ActType,
                activityStatus: request.ActivityStatus,
                employeeId: request.EmployeeId,
                limit: request.Limit,
                offset: request.Offset,
                startFrom: request.StartFrom,
                startTo: request.StartTo);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<ActivityListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} {Action}", Fn, action);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} {Action}", Fn, action);
            throw new InvalidOperationException("Activity list response could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? activityId = null,
        string? search = null,
        string? actType = null,
        string? activityStatus = null,
        int? employeeId = null,
        int? limit = null,
        int? offset = null,
        DateTime? startFrom = null,
        DateTime? startTo = null,
        string? subject = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? callNumber = null,
        string? callNotes = null,
        string? username = null,
        int? branchId = null,
        int? fiscalYearId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(activityId),
            Varchar(search),
            Varchar(actType),
            Varchar(activityStatus),
            NullableInt(employeeId),
            NullableInt(limit),
            NullableInt(offset),
            Timestamp(startFrom),
            Timestamp(startTo),
            Varchar(subject),
            Timestamp(startTime),
            Timestamp(endTime),
            Varchar(callNumber),
            Text(callNotes),
            Varchar(username),
            NullableInt(branchId),
            NullableInt(fiscalYearId));

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

    private static NpgsqlParameter Timestamp(DateTime? value) =>
        new()
        {
            Value = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified) : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Timestamp
        };
}
