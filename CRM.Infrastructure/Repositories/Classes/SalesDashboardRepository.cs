using System.Text.Json;
using CRM.Domain.DTOs.SalesDashboard.Requests;
using CRM.Domain.DTOs.SalesDashboard.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_sales_dashboard only — strictly no inline SQL.</summary>
public sealed class SalesDashboardRepository : ISalesDashboardRepository
{
    private const string Fn = "public.fn_crm_sales_dashboard";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<SalesDashboardRepository> _logger;

    public SalesDashboardRepository(ProductDatabaseHelper db, ILogger<SalesDashboardRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SalesDashboardResponseDto> GetSalesDashboardAsync(
        int orgId,
        int appId,
        GetSalesDashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "overview",
                orgId,
                appId,
                period: request.Period,
                dateFrom: request.DateFrom,
                dateTo: request.DateTo,
                ownerEmployeeId: request.OwnerEmployeeId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new SalesDashboardResponseDto();

            return JsonSerializer.Deserialize<SalesDashboardResponseDto>(json, PostgresJsonOptions.Options)
                   ?? new SalesDashboardResponseDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} overview", Fn);
            throw;
        }
    }

    public async Task<SalesDashboardLookupsResponseDto> GetLookupsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new SalesDashboardLookupsResponseDto();

            return JsonSerializer.Deserialize<SalesDashboardLookupsResponseDto>(json, PostgresJsonOptions.Options)
                   ?? new SalesDashboardLookupsResponseDto();
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
        string? period = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int? ownerEmployeeId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Varchar(period),
            NullableTimestampTz(dateFrom),
            NullableTimestampTz(dateTo),
            NullableInt(ownerEmployeeId));

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

    private static NpgsqlParameter NullableTimestampTz(DateTime? value) =>
        new()
        {
            Value = value.HasValue
                ? (value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime())
                : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.TimestampTz
        };
}

