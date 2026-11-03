using System.Text.Json;
using CRM.Domain.DTOs.Timeline.Requests;
using CRM.Domain.DTOs.Timeline.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_timeline only — no inline table SQL.</summary>
public sealed class TimelineRepository : ITimelineRepository
{
    private const string Fn = "public.fn_crm_timeline";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<TimelineRepository> _logger;

    public TimelineRepository(ProductDatabaseHelper db, ILogger<TimelineRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TimelineSummaryDto> GetSummaryAsync(
        int orgId, int appId, string entityType, long entityId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "summary", orgId, appId,
                entityType: entityType,
                entityId: entityId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new TimelineSummaryDto { AccountId = entityId };

            return JsonSerializer.Deserialize<TimelineSummaryDto>(json, PostgresJsonOptions.Options)
                   ?? new TimelineSummaryDto { AccountId = entityId };
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} summary", Fn);
            throw;
        }
    }

    public async Task<TimelineStatsDto> GetStatsAsync(
        int orgId, int appId, string entityType, long? entityId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "stats", orgId, appId,
                entityType: entityType,
                entityId: entityId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new TimelineStatsDto();

            return JsonSerializer.Deserialize<TimelineStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new TimelineStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<TimelineItemDto>> GetTimelineAsync(
        int orgId, int appId, GetTimelineRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "timeline", orgId, appId,
                entityType: request.EntityType,
                entityId: request.EntityId > 0 ? request.EntityId : null,
                interactionType: request.InteractionType,
                ownerId: request.OwnerId,
                search: request.Search,
                dateFrom: request.DateFrom,
                dateTo: request.DateTo,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<TimelineItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} timeline", Fn);
            throw;
        }
    }

    public async Task<LogInteractionResponse> LogInteractionAsync(
        int orgId, int appId, LogInteractionRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "log_interaction", orgId, appId,
                entityType: request.EntityType,
                entityId: request.EntityId > 0 ? request.EntityId : null,
                interactionType: request.InteractionType,
                ownerId: request.OwnerId,
                title: request.Title,
                description: request.Description,
                direction: request.Direction,
                contactName: request.ContactName,
                contactInfo: request.ContactInfo,
                durationMinutes: request.DurationMinutes,
                interactionTime: request.InteractionTime);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Log interaction returned empty response.");

            return JsonSerializer.Deserialize<LogInteractionResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Log interaction response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} log_interaction", Fn);
            throw;
        }
    }

    public async Task<TimelineLookupsResponse> GetLookupsAsync(
        int orgId, int appId, long? entityId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "lookups", orgId, appId,
                entityId: entityId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new TimelineLookupsResponse();

            return JsonSerializer.Deserialize<TimelineLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new TimelineLookupsResponse();
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
        string? entityType = null,
        long? entityId = null,
        string? interactionType = null,
        int? ownerId = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int? limit = null,
        int? offset = null,
        string? title = null,
        string? description = null,
        string? direction = null,
        string? contactName = null,
        string? contactInfo = null,
        int? durationMinutes = null,
        DateTime? interactionTime = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Varchar(entityType),
            NullableBigint(entityId),
            Varchar(interactionType),
            NullableInt(ownerId),
            Varchar(search),
            NullableTimestampTz(dateFrom),
            NullableTimestampTz(dateTo),
            NullableInt(limit),
            NullableInt(offset),
            Text(title),
            Text(description),
            Varchar(direction),
            Varchar(contactName),
            Varchar(contactInfo),
            NullableInt(durationMinutes),
            NullableTimestampTz(interactionTime));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableBigint(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

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

    private static NpgsqlParameter NullableTimestampTz(DateTime? value) =>
        new()
        {
            Value = value.HasValue ? (object)value.Value.ToUniversalTime() : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.TimestampTz
        };
}

