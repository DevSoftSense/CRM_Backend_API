using System.Text.Json;
using CRM.Domain.DTOs.Documents.Requests;
using CRM.Domain.DTOs.Documents.Responses;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CRM.Infrastructure.Repositories.Classes;

/// <summary>Calls public.fn_crm_document only — no inline table SQL.</summary>
public sealed class DocumentRepository : IDocumentRepository
{
    private const string Fn = "public.fn_crm_document";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<DocumentRepository> _logger;

    public DocumentRepository(ProductDatabaseHelper db, ILogger<DocumentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync(
        int orgId, int appId, GetDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                folder: request.Folder,
                fileType: request.FileType,
                ownerId: request.OwnerId,
                entityType: request.EntityType,
                entityId: request.EntityId,
                limit: request.Limit,
                offset: request.Offset,
                tab: request.Tab);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return [];
            return JsonSerializer.Deserialize<List<DocumentListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<DocumentStatsDto> GetStatsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("stats", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new DocumentStatsDto();
            return JsonSerializer.Deserialize<DocumentStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new DocumentStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats", Fn);
            throw;
        }
    }

    public async Task<DocumentLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("lookups", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new DocumentLookupsResponse();
            return JsonSerializer.Deserialize<DocumentLookupsResponse>(json, PostgresJsonOptions.Options)
                   ?? new DocumentLookupsResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} lookups", Fn);
            throw;
        }
    }

    public async Task<DocumentDetailDto> GetByIdAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, documentId: documentId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException($"Document {documentId} not found.");
            return JsonSerializer.Deserialize<DocumentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Document details could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<DocumentDetailDto> CreateDocumentAsync(
        int orgId, int appId, string fileName, string fileUrl, long fileSize,
        string? folder, string? description, string? entityType, long? entityId, int? ownerId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId,
                fileName: fileName,
                fileUrl: fileUrl,
                fileSize: fileSize,
                folder: folder,
                description: description,
                entityType: entityType,
                entityId: entityId,
                ownerId: ownerId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create document returned empty response.");
            return JsonSerializer.Deserialize<DocumentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create document response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<bool> CreateFolderAsync(
        int orgId, int appId, string folderName, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("create_folder", orgId, appId, folderName: folderName);
            return !string.IsNullOrWhiteSpace(json) && json != "null";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create_folder", Fn);
            throw;
        }
    }

    public async Task<DeactivateDocumentResponse> DeactivateDocumentAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("deactivate", orgId, appId, documentId: documentId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Deactivate document returned empty response.");
            return JsonSerializer.Deserialize<DeactivateDocumentResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Deactivate document response could not be parsed.");
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
        long? documentId = null,
        string? search = null,
        string? folder = null,
        string? fileType = null,
        int? ownerId = null,
        string? entityType = null,
        long? entityId = null,
        int? limit = null,
        int? offset = null,
        string? fileName = null,
        string? fileUrl = null,
        long? fileSize = null,
        string? description = null,
        string? tab = null,
        string? folderName = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableBigint(documentId),
            Varchar(search),
            Varchar(folder),
            Varchar(fileType),
            NullableInt(ownerId),
            Varchar(entityType),
            NullableBigint(entityId),
            NullableInt(limit),
            NullableInt(offset),
            Text(fileName),
            Text(fileUrl),
            NullableBigint(fileSize),
            Text(description),
            Varchar(tab),
            Text(folderName));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableBigint(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

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

