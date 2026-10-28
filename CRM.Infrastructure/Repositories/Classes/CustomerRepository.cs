using System.Text.Json;
using CRM.Domain.DTOs.Customers.Requests;
using CRM.Domain.DTOs.Customers.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_customer only — no inline table SQL.</summary>
public sealed class CustomerRepository : ICustomerRepository
{
    private const string Fn = "public.fn_crm_customer";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(ProductDatabaseHelper db, ILogger<CustomerRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(
        int orgId, int appId, GetCustomersRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                status: request.Status,
                accountTypeId: request.AccountTypeId,
                limit: request.Limit,
                offset: request.Offset);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<CustomerListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<CustomerDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, string? search, int limit, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("dropdown", orgId, appId, search: search, limit: limit);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<CustomerDropdownItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} dropdown", Fn);
            throw;
        }
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId, int appId, int accountId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, accountId: accountId);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<CustomerDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<CustomerDetailDto> CreateCustomerAsync(
        int orgId, int appId, int createdBy, CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                createdBy: createdBy,
                displayName: request.DisplayName,
                firstName: request.FirstName,
                lastName: request.LastName,
                companyName: request.CompanyName,
                email: request.Email,
                phoneMobile: request.PhoneMobile,
                phoneWork: request.PhoneWork,
                websiteUrl: request.WebsiteUrl,
                accountTypeId: request.AccountTypeId,
                partyType: request.PartyType,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId,
                remarks: request.Remarks);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create customer returned empty response.");
            return JsonSerializer.Deserialize<CustomerDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create customer response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<CustomerDetailDto> UpdateCustomerAsync(
        int orgId, int appId, int accountId, int updatedBy, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update", orgId, appId,
                accountId: accountId,
                updatedBy: updatedBy,
                status: request.Status,
                displayName: request.DisplayName,
                firstName: request.FirstName,
                lastName: request.LastName,
                companyName: request.CompanyName,
                email: request.Email,
                phoneMobile: request.PhoneMobile,
                phoneWork: request.PhoneWork,
                websiteUrl: request.WebsiteUrl,
                accountTypeId: request.AccountTypeId,
                partyType: request.PartyType,
                branchId: request.BranchId,
                fiscalYearId: request.FiscalYearId,
                remarks: request.Remarks);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update customer returned empty response.");
            return JsonSerializer.Deserialize<CustomerDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update customer response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update", Fn);
            throw;
        }
    }

    public async Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId, int appId, int accountId, int updatedBy, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, accountId: accountId, updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate customer returned empty response.");
            return JsonSerializer.Deserialize<DeactivateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate customer response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate", Fn);
            throw;
        }
    }

    public async Task<CustomerLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new CustomerLookupsResponse();
            return JsonSerializer.Deserialize<CustomerLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new CustomerLookupsResponse();
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
        int? accountId = null,
        string? search = null,
        string? status = null,
        int? accountTypeId = null,
        int? limit = null,
        int? offset = null,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? companyName = null,
        string? email = null,
        string? phoneMobile = null,
        string? phoneWork = null,
        string? websiteUrl = null,
        string? partyType = null,
        int? branchId = null,
        int? fiscalYearId = null,
        string? remarks = null,
        int? createdBy = null,
        int? updatedBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(accountId),
            Varchar(search),
            Varchar(status),
            NullableInt(accountTypeId),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(displayName),
            Varchar(firstName),
            Varchar(lastName),
            Varchar(companyName),
            Varchar(email),
            Varchar(phoneMobile),
            Varchar(phoneWork),
            Varchar(websiteUrl),
            Varchar(partyType),
            NullableInt(branchId),
            NullableInt(fiscalYearId),
            Text(remarks),
            NullableInt(createdBy),
            NullableInt(updatedBy));

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
}
