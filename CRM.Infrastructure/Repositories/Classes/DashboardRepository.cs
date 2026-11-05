using System.Text.Json;
using CRM.Domain.DTOs.Dashboard.Requests;
using CRM.Domain.DTOs.Dashboard.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_dashboard only — no inline table SQL.</summary>
public sealed class DashboardRepository : IDashboardRepository
{
    private const string Fn = "public.fn_crm_dashboard";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(ProductDatabaseHelper db, ILogger<DashboardRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CrmDashboardResponseDto> GetDashboardAsync(
        int orgId, int appId, GetDashboardRequest request, CancellationToken cancellationToken = default)
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
                return new CrmDashboardResponseDto();

            var result = JsonSerializer.Deserialize<CrmDashboardResponseDto>(json, PostgresJsonOptions.Options)
                   ?? new CrmDashboardResponseDto();

            if (result.TodaysSchedule != null)
            {
                foreach (var s in result.TodaysSchedule)
                {
                    if (string.IsNullOrWhiteSpace(s.TimeLabel) && s.StartTime.HasValue)
                        s.TimeLabel = s.StartTime.Value.ToString("hh:mm tt");

                    if (string.IsNullOrWhiteSpace(s.ActivityType))
                        s.ActivityType = !string.IsNullOrWhiteSpace(s.Type) ? s.Type : "Meeting";

                    if (string.IsNullOrWhiteSpace(s.CustomerName))
                    {
                        if (!string.IsNullOrWhiteSpace(s.Notes))
                        {
                            var m = System.Text.RegularExpressions.Regex.Match(s.Notes, @"Related:\s*([^\|\n\]]+)");
                            if (m.Success)
                                s.CustomerName = m.Groups[1].Value.Trim();
                            else
                            {
                                var mw = System.Text.RegularExpressions.Regex.Match(s.Notes, @"With:\s*([^\|\n\]]+)");
                                if (mw.Success)
                                    s.CustomerName = mw.Groups[1].Value.Trim();
                            }
                        }
                        if (string.IsNullOrWhiteSpace(s.CustomerName))
                            s.CustomerName = !string.IsNullOrWhiteSpace(s.Subject) ? s.Subject : "Client";
                    }
                }
            }

            return result;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} overview", Fn);
            throw;
        }
    }

    public async Task<DashboardLookupsResponseDto> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);

            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
                return new DashboardLookupsResponseDto();

            return JsonSerializer.Deserialize<DashboardLookupsResponseDto>(json, PostgresJsonOptions.Options)
                   ?? new DashboardLookupsResponseDto();
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
            Value = value.HasValue ? (object)value.Value.ToUniversalTime() : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.TimestampTz
        };
}

