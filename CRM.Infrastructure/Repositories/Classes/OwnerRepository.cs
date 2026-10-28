using System.Text.Json;
using CRM.Domain.DTOs.Owners.Requests;
using CRM.Domain.DTOs.Owners.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_owner only — no inline table SQL.</summary>
public sealed class OwnerRepository : IOwnerRepository
{
    private const string Fn = "public.fn_crm_owner";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<OwnerRepository> _logger;

    public OwnerRepository(ProductDatabaseHelper db, ILogger<OwnerRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OwnerListItemDto>> GetOwnersAsync(
        int orgId, int appId, GetOwnersRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                status: request.Status,
                limit: request.Limit,
                offset: request.Offset);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<OwnerListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<OwnerDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, string? search, int limit, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("dropdown", orgId, appId, search: search, limit: limit);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<OwnerDropdownItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} dropdown", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<OwnerDropdownItemDto>> EnsureSeedAsync(
        int orgId, int appId, int? createdBy, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("ensure_seed", orgId, appId, createdBy: createdBy, limit: 200);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<OwnerDropdownItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} ensure_seed", Fn);
            throw;
        }
    }

    public async Task<OwnerDetailDto?> GetOwnerByIdAsync(
        int orgId, int appId, int ownerId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, ownerId: ownerId);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<OwnerDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<OwnerDetailDto> CreateOwnerAsync(
        int orgId, int appId, int createdBy, CreateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                createdBy: createdBy,
                fullName: request.FullName,
                email: request.Email,
                phone: request.Phone,
                ownerCode: request.OwnerCode,
                branchId: request.BranchId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create owner returned empty response.");
            return JsonSerializer.Deserialize<OwnerDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create owner response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<OwnerDetailDto> UpdateOwnerAsync(
        int orgId, int appId, int ownerId, int updatedBy, UpdateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update", orgId, appId,
                ownerId: ownerId,
                updatedBy: updatedBy,
                status: request.Status,
                fullName: request.FullName,
                email: request.Email,
                phone: request.Phone,
                ownerCode: request.OwnerCode,
                branchId: request.BranchId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update owner returned empty response.");
            return JsonSerializer.Deserialize<OwnerDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update owner response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update", Fn);
            throw;
        }
    }

    public async Task<DeactivateOwnerResponse> DeactivateOwnerAsync(
        int orgId, int appId, int ownerId, int updatedBy, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, ownerId: ownerId, updatedBy: updatedBy);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate owner returned empty response.");
            return JsonSerializer.Deserialize<DeactivateOwnerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate owner response could not be parsed.");
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
        int? ownerId = null,
        string? search = null,
        int? limit = null,
        int? offset = null,
        string? status = null,
        string? fullName = null,
        string? email = null,
        string? phone = null,
        string? ownerCode = null,
        int? branchId = null,
        int? createdBy = null,
        int? updatedBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(ownerId),
            Varchar(search),
            NullableInt(limit),
            NullableInt(offset),
            Varchar(status),
            Varchar(fullName),
            Varchar(email),
            Varchar(phone),
            Varchar(ownerCode),
            NullableInt(branchId),
            NullableLong(createdBy),
            NullableLong(updatedBy));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableLong(int? value) =>
        new() { Value = value.HasValue ? (long)value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };
}
