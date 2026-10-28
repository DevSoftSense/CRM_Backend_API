using System.Text.Json;
using CRM.Domain.DTOs.Contacts.Requests;
using CRM.Domain.DTOs.Contacts.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_contact only — no inline table SQL.</summary>
public sealed class ContactRepository : IContactRepository
{
    private const string Fn = "public.fn_crm_contact";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ContactRepository> _logger;

    public ContactRepository(ProductDatabaseHelper db, ILogger<ContactRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContactListItemDto>> GetContactsAsync(
        int orgId, int appId, GetContactsRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                accountId: request.AccountId,
                search: request.Search,
                isPrimary: request.IsPrimary,
                limit: request.Limit,
                offset: request.Offset);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<ContactListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<ContactDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, int? accountId, string? search, int limit, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("dropdown", orgId, appId, accountId: accountId, search: search, limit: limit);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<ContactDropdownItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} dropdown", Fn);
            throw;
        }
    }

    public async Task<ContactDetailDto?> GetContactByIdAsync(
        int orgId, int appId, int contactPersonId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, contactPersonId: contactPersonId);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<ContactDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<ContactDetailDto> CreateContactAsync(
        int orgId, int appId, CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                accountId: request.AccountId,
                salutation: request.Salutation,
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                mobile: request.Mobile,
                workPhone: request.WorkPhone,
                isPrimary: request.IsPrimary,
                fiscalYearId: request.FiscalYearId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create contact returned empty response.");
            return JsonSerializer.Deserialize<ContactDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create contact response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<ContactDetailDto> UpdateContactAsync(
        int orgId, int appId, int contactPersonId, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update", orgId, appId,
                contactPersonId: contactPersonId,
                accountId: request.AccountId,
                salutation: request.Salutation,
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                mobile: request.Mobile,
                workPhone: request.WorkPhone,
                isPrimary: request.IsPrimary,
                fiscalYearId: request.FiscalYearId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update contact returned empty response.");
            return JsonSerializer.Deserialize<ContactDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update contact response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update", Fn);
            throw;
        }
    }

    public async Task<DeactivateContactResponse> DeactivateContactAsync(
        int orgId, int appId, int contactPersonId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, contactPersonId: contactPersonId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate contact returned empty response.");
            return JsonSerializer.Deserialize<DeactivateContactResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate contact response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate", Fn);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? contactPersonId = null,
        int? accountId = null,
        string? search = null,
        bool? isPrimary = null,
        int? limit = null,
        int? offset = null,
        string? salutation = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? mobile = null,
        string? workPhone = null,
        int? fiscalYearId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(contactPersonId),
            NullableInt(accountId),
            Varchar(search),
            NullableBool(isPrimary),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(salutation),
            Varchar(firstName),
            Varchar(lastName),
            Varchar(email),
            Varchar(mobile),
            Varchar(workPhone),
            NullableInt(fiscalYearId));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableBool(bool? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Boolean };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };
}
