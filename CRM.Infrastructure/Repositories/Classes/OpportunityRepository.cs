using System.Text.Json;
using CRM.Domain.DTOs.Opportunities.Requests;
using CRM.Domain.DTOs.Opportunities.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls public.fn_crm_opportunity(p_action, p_org_id, p_app_id, …) only — no inline table SQL.
/// </summary>
public sealed class OpportunityRepository : IOpportunityRepository
{
    private const string Fn = "public.fn_crm_opportunity";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<OpportunityRepository> _logger;

    public OpportunityRepository(ProductDatabaseHelper db, ILogger<OpportunityRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OpportunityListItemDto>> GetOpportunitiesAsync(
        int orgId,
        int appId,
        GetOpportunitiesRequest request,
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
                stageId: request.StageId,
                opportunityTypeId: request.OpportunityTypeId,
                sourceId: request.SourceId,
                priorityId: request.PriorityId,
                ownerEmployeeId: request.OwnerEmployeeId,
                accountId: request.AccountId,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<OpportunityListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} list for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Opportunity list response could not be parsed.", ex);
        }
    }

    public async Task<OpportunityDetailDto?> GetOpportunityByIdAsync(
        int orgId,
        int appId,
        long opportunityId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, opportunityId: opportunityId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<OpportunityDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for opportunityId {OpportunityId}", Fn, opportunityId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for opportunityId {OpportunityId}", Fn, opportunityId);
            throw new InvalidOperationException("Opportunity response could not be parsed.", ex);
        }
    }

    public async Task<OpportunityDetailDto> CreateOpportunityAsync(
        int orgId,
        int appId,
        long createdBy,
        CreateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                createdBy: createdBy,
                name: request.Name,
                accountId: request.AccountId,
                primaryContactId: request.PrimaryContactId,
                leadId: request.LeadId,
                campaignId: request.CampaignId,
                stageId: request.StageId,
                opportunityTypeId: request.OpportunityTypeId,
                sourceId: request.SourceId,
                amount: request.Amount,
                currencyCode: request.CurrencyCode,
                probability: request.Probability,
                expectedCloseDate: request.ExpectedCloseDate,
                actualCloseDate: request.ActualCloseDate,
                forecastCategoryId: request.ForecastCategoryId,
                priorityId: request.PriorityId,
                nextStep: request.NextStep,
                closedReason: request.ClosedReason,
                ownerEmployeeId: request.OwnerEmployeeId,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create opportunity returned empty response.");

            return JsonSerializer.Deserialize<OpportunityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create opportunity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<OpportunityDetailDto> UpdateOpportunityAsync(
        int orgId,
        int appId,
        long opportunityId,
        long updatedBy,
        UpdateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                opportunityId: opportunityId,
                updatedBy: updatedBy,
                name: request.Name,
                accountId: request.AccountId,
                primaryContactId: request.PrimaryContactId,
                leadId: request.LeadId,
                campaignId: request.CampaignId,
                stageId: request.StageId,
                opportunityTypeId: request.OpportunityTypeId,
                sourceId: request.SourceId,
                amount: request.Amount,
                currencyCode: request.CurrencyCode,
                probability: request.Probability,
                expectedCloseDate: request.ExpectedCloseDate,
                actualCloseDate: request.ActualCloseDate,
                forecastCategoryId: request.ForecastCategoryId,
                priorityId: request.PriorityId,
                nextStep: request.NextStep,
                closedReason: request.ClosedReason,
                ownerEmployeeId: request.OwnerEmployeeId,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update opportunity returned empty response.");

            return JsonSerializer.Deserialize<OpportunityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update opportunity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update for opportunityId {OpportunityId}", Fn, opportunityId);
            throw;
        }
    }

    public async Task<DeactivateOpportunityResponse> DeactivateOpportunityAsync(
        int orgId,
        int appId,
        long opportunityId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, opportunityId: opportunityId, updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate opportunity returned empty response.");

            return JsonSerializer.Deserialize<DeactivateOpportunityResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate opportunity response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate for opportunityId {OpportunityId}", Fn, opportunityId);
            throw;
        }
    }

    public async Task<OpportunityLookupsResponse> GetLookupsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new OpportunityLookupsResponse();

            return JsonSerializer.Deserialize<OpportunityLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new OpportunityLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<PipelineBoardResponse> GetPipelineAsync(
        int orgId, int appId, string? search, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("pipeline", orgId, appId, search: search);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new PipelineBoardResponse();
            return JsonSerializer.Deserialize<PipelineBoardResponse>(json, PostgresJsonOptions.Options)
                   ?? new PipelineBoardResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} pipeline for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<OpportunityDetailDto> MoveStageAsync(
        int orgId, int appId, long opportunityId, int stageId, long updatedBy, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "move_stage", orgId, appId,
                opportunityId: opportunityId,
                stageId: stageId,
                updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Move stage returned empty response.");
            return JsonSerializer.Deserialize<OpportunityDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Move stage response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} move_stage for opportunityId {OpportunityId}", Fn, opportunityId);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? opportunityId = null,
        string? search = null,
        int? stageId = null,
        int? opportunityTypeId = null,
        int? sourceId = null,
        int? priorityId = null,
        int? ownerEmployeeId = null,
        int? accountId = null,
        int? limit = null,
        int? offset = null,
        string? name = null,
        int? primaryContactId = null,
        long? leadId = null,
        long? campaignId = null,
        int? forecastCategoryId = null,
        decimal? amount = null,
        string? currencyCode = null,
        decimal? probability = null,
        DateOnly? expectedCloseDate = null,
        DateOnly? actualCloseDate = null,
        string? nextStep = null,
        string? closedReason = null,
        int? branchId = null,
        int? fiscalYearId = null,
        long? createdBy = null,
        long? updatedBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableBigInt(opportunityId),
            Varchar(search),
            NullableInt(stageId),
            NullableInt(opportunityTypeId),
            NullableInt(sourceId),
            NullableInt(priorityId),
            NullableInt(ownerEmployeeId),
            NullableInt(accountId),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(name),
            NullableInt(primaryContactId),
            NullableBigInt(leadId),
            NullableBigInt(campaignId),
            NullableInt(forecastCategoryId),
            NullableNumeric(amount),
            Varchar(currencyCode),
            NullableNumeric(probability),
            Date(expectedCloseDate),
            Date(actualCloseDate),
            Varchar(nextStep),
            Text(closedReason),
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
