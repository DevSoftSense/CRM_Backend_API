using System.Text;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Executes PostgreSQL <c>fn_*</c> on the organisation product DB (SOC_SaaS_Product).
/// <para>
/// SoftOnCloud:UseProductConnectionDb = false → local SecondConnection (dev)
/// SoftOnCloud:UseProductConnectionDb = true  → SoftOnCloud product-connection (JWT staff)
/// </para>
/// Repositories must call functions only — no inline SQL.
/// </summary>
public sealed class ProductDatabaseHelper
{
    private readonly ITenantConnectionFactory _tenantConnections;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _useProductConnectionApi;
    private readonly string? _localConnectionString;
    private readonly ILogger<ProductDatabaseHelper> _logger;

    public ProductDatabaseHelper(
        ITenantConnectionFactory tenantConnections,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<ProductDatabaseHelper> logger)
    {
        _tenantConnections = tenantConnections;
        _httpContextAccessor = httpContextAccessor;
        _useProductConnectionApi = configuration.GetValue<bool>("SoftOnCloud:UseProductConnectionDb");
        _localConnectionString = configuration.GetConnectionString("SecondConnection")
                                 ?? configuration.GetConnectionString("DefaultConnection");
        _logger = logger;
    }

    /// <summary>
    /// Calls a PostgreSQL function that already returns JSONB/JSON (e.g. unified module fn with p_action).
    /// </summary>
    public async Task<string> ExecuteJsonFunctionAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildJsonFunctionSql(functionName, parameters.Length);

        try
        {
            await using var connection = await OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "null" : result.ToString() ?? "null";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing JSON function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing JSON function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<string> ExecuteSingleRowTableFunctionAsJsonAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildSingleRowTableFunctionToJsonSql(functionName, parameters.Length);

        try
        {
            await using var connection = await OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "null" : result.ToString() ?? "null";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<string> ExecuteTableFunctionAsJsonArrayAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildTableFunctionToJsonArraySql(functionName, parameters.Length);

        try
        {
            await using var connection = await OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "[]" : result.ToString() ?? "[]";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing function {FunctionName}", functionName);
            throw;
        }
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await ResolveConnectionStringAsync(cancellationToken);
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private async Task<string> ResolveConnectionStringAsync(CancellationToken cancellationToken)
    {
        if (!_useProductConnectionApi)
        {
            if (string.IsNullOrWhiteSpace(_localConnectionString))
            {
                throw new InvalidOperationException(
                    "SoftOnCloud:UseProductConnectionDb is false, but ConnectionStrings:SecondConnection is not set. " +
                    "Add your local SOC_SaaS_Product connection string.");
            }

            return _localConnectionString;
        }

        if (HasSoftOnCloudBearer())
        {
            var connectionString = await _tenantConnections.GetResolvedConnectionAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "SoftOnCloud product-connection returned an empty connection string.");
            }

            return connectionString;
        }

        if (!string.IsNullOrWhiteSpace(_localConnectionString))
        {
            _logger.LogWarning(
                "No SoftOnCloud JWT; using ConnectionStrings:SecondConnection (expected for local workers).");
            return _localConnectionString;
        }

        throw new InvalidOperationException(
            "Cannot resolve product DB: no SoftOnCloud JWT and ConnectionStrings:SecondConnection is not set.");
    }

    private bool HasSoftOnCloudBearer()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        return !string.IsNullOrWhiteSpace(authHeader)
               && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildJsonFunctionSql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")::text");
        return sb.ToString();
    }

    private static string BuildSingleRowTableFunctionToJsonSql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT row_to_json(t)::text ");
        sb.Append("FROM (SELECT * FROM ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")) AS t LIMIT 1");
        return sb.ToString();
    }

    private static string BuildTableFunctionToJsonArraySql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT COALESCE(json_agg(t), '[]'::json)::text ");
        sb.Append("FROM (SELECT * FROM ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")) AS t");
        return sb.ToString();
    }

    private static void AppendPositionalParams(StringBuilder sb, int paramCount)
    {
        for (var i = 0; i < paramCount; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('$').Append(i + 1);
        }
    }

    private static void ValidateFunctionName(string functionName)
    {
        foreach (var ch in functionName)
        {
            if (!(char.IsLetterOrDigit(ch) || ch is '_' or '.'))
                throw new ArgumentException($"Invalid PostgreSQL function name: {functionName}", nameof(functionName));
        }
    }
}
