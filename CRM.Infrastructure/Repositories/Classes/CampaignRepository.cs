using System.Text.Json;
using CRM.Domain.DTOs.Campaigns.Requests;
using CRM.Domain.DTOs.Campaigns.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls public.fn_crm_campaign(p_action, p_org_id, p_app_id, ...) only — zero inline SQL.
/// </summary>
public sealed class CampaignRepository : ICampaignRepository
{
    private const string Fn = "public.fn_crm_campaign";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<CampaignRepository> _logger;

    public CampaignRepository(ProductDatabaseHelper db, ILogger<CampaignRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CampaignListItemDto>> GetCampaignsAsync(
        int orgId,
        int appId,
        GetCampaignsRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list",
                orgId,
                appId,
                search: request.Search,
                channelId: request.ChannelId,
                statusId: request.StatusId,
                ownerEmployeeId: request.OwnerEmployeeId,
                startFrom: request.StartFrom,
                startTo: request.StartTo,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<CampaignListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<CampaignDetailDto?> GetCampaignByIdAsync(
        int orgId,
        int appId,
        long campaignId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, campaignId: campaignId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<CampaignDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for campaignId {CampaignId}", Fn, campaignId);
            throw;
        }
    }

    public async Task<CampaignDetailDto> CreateCampaignAsync(
        int orgId,
        int appId,
        long createdBy,
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                campaignName: request.CampaignName,
                channelId: request.ChannelId,
                statusId: request.StatusId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                budget: request.Budget,
                currencyCode: request.CurrencyCode,
                ownerEmployeeId: request.OwnerEmployeeId,
                description: request.Description,
                createdBy: createdBy);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create campaign returned an empty response.");

            return JsonSerializer.Deserialize<CampaignDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Could not deserialize created campaign.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<CampaignDetailDto> UpdateCampaignAsync(
        int orgId,
        int appId,
        long campaignId,
        long updatedBy,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                campaignId: campaignId,
                campaignName: request.CampaignName,
                channelId: request.ChannelId,
                statusId: request.StatusId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                budget: request.Budget,
                currencyCode: request.CurrencyCode,
                ownerEmployeeId: request.OwnerEmployeeId,
                description: request.Description,
                updatedBy: updatedBy);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update campaign returned an empty response.");

            return JsonSerializer.Deserialize<CampaignDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Could not deserialize updated campaign.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update for campaignId {CampaignId}", Fn, campaignId);
            throw;
        }
    }

    public async Task<DeactivateCampaignResponse> DeactivateCampaignAsync(
        int orgId,
        int appId,
        long campaignId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, campaignId: campaignId, updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate campaign returned an empty response.");

            return JsonSerializer.Deserialize<DeactivateCampaignResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Could not deserialize deactivate campaign response.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate for campaignId {CampaignId}", Fn, campaignId);
            throw;
        }
    }

    public async Task<CampaignLookupsResponse> GetLookupsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new CampaignLookupsResponse();

            return JsonSerializer.Deserialize<CampaignLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new CampaignLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<IReadOnlyList<CampaignDropdownItemDto>> GetDropdownAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("dropdown", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<CampaignDropdownItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} dropdown for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<CampaignStatsResponse> GetStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("stats", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new CampaignStatsResponse();

            return JsonSerializer.Deserialize<CampaignStatsResponse>(json, PostgresJsonOptions.Options)
                   ?? new CampaignStatsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? campaignId = null,
        string? search = null,
        int? channelId = null,
        int? statusId = null,
        int? ownerEmployeeId = null,
        DateOnly? startFrom = null,
        DateOnly? startTo = null,
        int? limit = null,
        int? offset = null,
        string? campaignName = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal? budget = null,
        string? currencyCode = null,
        string? description = null,
        int? branchId = null,
        int? fiscalYearId = null,
        long? createdBy = null,
        long? updatedBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableBigInt(campaignId),
            Varchar(search),
            NullableInt(channelId),
            NullableInt(statusId),
            NullableInt(ownerEmployeeId),
            Date(startFrom),
            Date(startTo),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(campaignName),
            Date(startDate),
            Date(endDate),
            NullableNumeric(budget),
            Varchar(currencyCode),
            Text(description),
            NullableInt(branchId),
            NullableInt(fiscalYearId),
            NullableBigInt(createdBy),
            NullableBigInt(updatedBy));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableBigInt(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

    private static NpgsqlParameter NullableNumeric(decimal? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Numeric };

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

