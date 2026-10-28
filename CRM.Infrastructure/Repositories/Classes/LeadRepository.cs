using System.Text.Json;
using CRM.Domain.DTOs.Leads.Requests;
using CRM.Domain.DTOs.Leads.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls public.fn_crm_lead(p_action, p_org_id, p_app_id, …) only — no inline table SQL.
/// </summary>
public sealed class LeadRepository : ILeadRepository
{
    private const string Fn = "public.fn_crm_lead";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<LeadRepository> _logger;

    public LeadRepository(ProductDatabaseHelper db, ILogger<LeadRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LeadListItemDto>> GetLeadsAsync(
        int orgId,
        int appId,
        GetLeadsRequest request,
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
                statusId: request.StatusId,
                sourceId: request.SourceId,
                ratingId: request.RatingId,
                ownerEmployeeId: request.OwnerEmployeeId,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<LeadListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} list for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Lead list response could not be parsed.", ex);
        }
    }

    public async Task<LeadDetailDto?> GetLeadByIdAsync(
        int orgId,
        int appId,
        long leadId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, leadId: leadId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<LeadDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for leadId {LeadId}", Fn, leadId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for leadId {LeadId}", Fn, leadId);
            throw new InvalidOperationException("Lead response could not be parsed.", ex);
        }
    }

    public async Task<LeadDetailDto> CreateLeadAsync(
        int orgId,
        int appId,
        long createdBy,
        CreateLeadRequest request,
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
                leadName: request.LeadName,
                companyName: request.CompanyName,
                designation: request.Designation,
                industry: request.Industry,
                email: request.Email,
                mobile: request.Mobile,
                phone: request.Phone,
                website: request.Website,
                sourceId: request.SourceId,
                statusId: request.StatusId,
                ratingId: request.RatingId,
                estimatedValue: request.EstimatedValue,
                currencyCode: request.CurrencyCode,
                expectedCloseDate: request.ExpectedCloseDate,
                description: request.Description,
                ownerEmployeeId: request.OwnerEmployeeId,
                campaignId: request.CampaignId,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create lead returned empty response.");

            return JsonSerializer.Deserialize<LeadDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create lead response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<LeadDetailDto> UpdateLeadAsync(
        int orgId,
        int appId,
        long leadId,
        long updatedBy,
        UpdateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                leadId: leadId,
                updatedBy: updatedBy,
                leadName: request.LeadName,
                companyName: request.CompanyName,
                designation: request.Designation,
                industry: request.Industry,
                email: request.Email,
                mobile: request.Mobile,
                phone: request.Phone,
                website: request.Website,
                sourceId: request.SourceId,
                statusId: request.StatusId,
                ratingId: request.RatingId,
                estimatedValue: request.EstimatedValue,
                currencyCode: request.CurrencyCode,
                expectedCloseDate: request.ExpectedCloseDate,
                description: request.Description,
                ownerEmployeeId: request.OwnerEmployeeId,
                campaignId: request.CampaignId,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update lead returned empty response.");

            return JsonSerializer.Deserialize<LeadDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update lead response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update for leadId {LeadId}", Fn, leadId);
            throw;
        }
    }

    public async Task<DeactivateLeadResponse> DeactivateLeadAsync(
        int orgId,
        int appId,
        long leadId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, leadId: leadId, updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate lead returned empty response.");

            return JsonSerializer.Deserialize<DeactivateLeadResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate lead response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate for leadId {LeadId}", Fn, leadId);
            throw;
        }
    }

    public async Task<LeadLookupsResponse> GetLookupsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new LeadLookupsResponse();

            return JsonSerializer.Deserialize<LeadLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new LeadLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? leadId = null,
        string? search = null,
        int? statusId = null,
        int? sourceId = null,
        int? ratingId = null,
        int? ownerEmployeeId = null,
        int? limit = null,
        int? offset = null,
        string? leadName = null,
        string? companyName = null,
        string? designation = null,
        string? industry = null,
        string? email = null,
        string? mobile = null,
        string? phone = null,
        string? website = null,
        decimal? estimatedValue = null,
        string? currencyCode = null,
        DateOnly? expectedCloseDate = null,
        string? description = null,
        long? campaignId = null,
        int? branchId = null,
        int? fiscalYearId = null,
        long? createdBy = null,
        long? updatedBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableBigInt(leadId),
            Varchar(search),
            NullableInt(statusId),
            NullableInt(sourceId),
            NullableInt(ratingId),
            NullableInt(ownerEmployeeId),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(leadName),
            Varchar(companyName),
            Varchar(designation),
            Varchar(industry),
            Varchar(email),
            Varchar(mobile),
            Varchar(phone),
            Varchar(website),
            NullableNumeric(estimatedValue),
            Varchar(currencyCode),
            Date(expectedCloseDate),
            Text(description),
            NullableBigInt(campaignId),
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
