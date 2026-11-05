using System.Text.Json;
using CRM.Domain.DTOs.Reports.Requests;
using CRM.Domain.DTOs.Reports.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_report only — strictly no inline SQL.</summary>
public sealed class ReportRepository : IReportRepository
{
    private const string Fn = "public.fn_crm_report";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(ProductDatabaseHelper db, ILogger<ReportRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ReportCatalogResponseDto> GetCatalogAsync(int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("catalog", orgId, appId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new ReportCatalogResponseDto();

            return JsonSerializer.Deserialize<ReportCatalogResponseDto>(json, PostgresJsonOptions.Options)
                   ?? new ReportCatalogResponseDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} catalog", Fn);
            throw;
        }
    }

    public async Task<ReportExecutionResultDto> RunReportAsync(
        int orgId,
        int appId,
        RunReportRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "run",
                orgId,
                appId,
                reportCode: request.ReportCode,
                dateFrom: request.DateFrom,
                dateTo: request.DateTo,
                ownerEmployeeId: request.OwnerEmployeeId,
                extraFilter: request.ExtraFilter);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new ReportExecutionResultDto { ReportCode = request.ReportCode };

            return JsonSerializer.Deserialize<ReportExecutionResultDto>(json, PostgresJsonOptions.Options)
                   ?? new ReportExecutionResultDto { ReportCode = request.ReportCode };
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} run ({Code})", Fn, request.ReportCode);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        string? reportCode = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int? ownerEmployeeId = null,
        string? extraFilter = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Varchar(reportCode),
            NullableTimestampTz(dateFrom),
            NullableTimestampTz(dateTo),
            NullableInt(ownerEmployeeId),
            NullableJsonb(extraFilter));

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

    private static NpgsqlParameter NullableJsonb(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };
}

