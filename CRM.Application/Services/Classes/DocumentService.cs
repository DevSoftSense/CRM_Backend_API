using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Documents.Requests;
using CRM.Domain.DTOs.Documents.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public DocumentService(
        IDocumentRepository documentRepository,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _documentRepository = documentRepository;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<DocumentListResponse> GetDocumentsAsync(
        int orgId, GetDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetDocumentsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var items = await _documentRepository.GetDocumentsAsync(orgId, GetAppId(), request, cancellationToken);
        return new DocumentListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0,
            Limit = request.Limit,
            Offset = request.Offset
        };
    }

    public async Task<DocumentStatsDto> GetStatsAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _documentRepository.GetStatsAsync(orgId, GetAppId(), cancellationToken);
    }

    public async Task<DocumentLookupsResponse> GetLookupsAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _documentRepository.GetLookupsAsync(orgId, GetAppId(), cancellationToken);
    }

    public async Task<DocumentDetailDto?> GetByIdAsync(
        int orgId, long documentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (documentId <= 0) throw new ArgumentException("Valid document ID is required.", nameof(documentId));
        return await _documentRepository.GetByIdAsync(orgId, GetAppId(), documentId, cancellationToken);
    }

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv",
        ".ppt", ".pptx", ".zip", ".rar", ".7z",
        ".png", ".jpg", ".jpeg", ".txt"
    };

    public async Task<DocumentDetailDto> UploadDocumentAsync(
        int orgId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request == null) throw new ArgumentNullException(nameof(request));

        var appId = GetAppId();
        var folder = string.IsNullOrWhiteSpace(request.Folder) ? "Proposals" : request.Folder.Trim();
        var safeFolder = SanitizePathSegment(folder, "Proposals");

        var rawFileName = !string.IsNullOrWhiteSpace(request.FileName)
            ? request.FileName.Trim()
            : (request.File != null ? Path.GetFileName(request.File.FileName) : string.Empty);

        if (string.IsNullOrWhiteSpace(rawFileName))
            throw new ArgumentException("Document name is required.");

        if (request.File == null || request.File.Length <= 0)
            throw new ArgumentException("A non-empty file is required.");

        var maxMb = Math.Clamp(_configuration.GetValue("CRM:Documents:MaxFileSizeMb", 50), 1, 200);
        var maxBytes = maxMb * 1024L * 1024L;
        if (request.File.Length > maxBytes)
            throw new ArgumentException($"File exceeds maximum allowed size of {maxMb} MB.");

        var originalFileName = SanitizePathSegment(Path.GetFileName(request.File.FileName), "document");
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException(
                "Unsupported file type. Allowed types: PDF, DOC/DOCX, XLS/XLSX/CSV, PPT/PPTX, ZIP/RAR/7Z, PNG, JPG/JPEG, TXT.");
        }

        // Store privately OUTSIDE wwwroot, strictly segmented by org_id and app_id
        var privateRoot = GetPrivateStorageRoot();
        var tenantFolderDir = Path.GetFullPath(
            Path.Combine(privateRoot, $"org_{orgId}", $"app_{appId}", safeFolder));
        EnsurePathWithinRoot(tenantFolderDir, privateRoot);
        Directory.CreateDirectory(tenantFolderDir);

        var uniquePrefix = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}"[..24];
        var storedFileName = $"{uniquePrefix}_{originalFileName}";
        var physicalPath = Path.GetFullPath(Path.Combine(tenantFolderDir, storedFileName));
        EnsurePathWithinRoot(physicalPath, privateRoot);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var fileUrl = $"private://org_{orgId}/app_{appId}/{safeFolder}/{storedFileName}";

        return await _documentRepository.CreateDocumentAsync(
            orgId,
            appId,
            rawFileName,
            fileUrl,
            request.File.Length,
            folder,
            request.Description,
            request.EntityType,
            request.EntityId,
            request.OwnerId,
            cancellationToken);
    }

    public async Task<bool> CreateFolderAsync(
        int orgId, string folderName, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (string.IsNullOrWhiteSpace(folderName))
            throw new ArgumentException("Folder name is required.", nameof(folderName));
        return await _documentRepository.CreateFolderAsync(orgId, GetAppId(), folderName.Trim(), cancellationToken);
    }

    public async Task<DeactivateDocumentResponse> DeactivateDocumentAsync(
        int orgId, long documentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (documentId <= 0) throw new ArgumentException("Valid document ID is required.", nameof(documentId));

        var appId = GetAppId();
        var existing = await _documentRepository.GetByIdAsync(orgId, appId, documentId, cancellationToken);
        var result = await _documentRepository.DeactivateDocumentAsync(orgId, appId, documentId, cancellationToken);

        // Clean up physical file on disk after DB deletion succeeds
        if (existing != null && !string.IsNullOrWhiteSpace(existing.FileUrl))
        {
            var physicalPath = TryResolvePhysicalPath(orgId, appId, existing.FileUrl);
            if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
            {
                try { File.Delete(physicalPath); } catch { /* ignore cleanup failure */ }
            }
        }

        return result;
    }

    public async Task<(string PhysicalPath, string FileName, string ContentType)?> GetFileDownloadAsync(
        int orgId, long documentId, CancellationToken cancellationToken = default)
    {
        var appId = GetAppId();
        var doc = await _documentRepository.GetByIdAsync(orgId, appId, documentId, cancellationToken);
        if (doc == null || string.IsNullOrWhiteSpace(doc.FileUrl)) return null;

        var physicalPath = TryResolvePhysicalPath(orgId, appId, doc.FileUrl);
        if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
        {
            return null;
        }

        // Preserve stored file extension when user gave a human title without extension
        var storedExt = Path.GetExtension(physicalPath);
        var downloadName = string.IsNullOrWhiteSpace(doc.FileName)
            ? Path.GetFileName(physicalPath)
            : doc.FileName.Trim();
        if (!string.IsNullOrWhiteSpace(storedExt) && string.IsNullOrWhiteSpace(Path.GetExtension(downloadName)))
        {
            downloadName += storedExt;
        }

        var contentType = GetContentType(string.IsNullOrWhiteSpace(storedExt) ? downloadName : physicalPath);
        return (physicalPath, downloadName, contentType);
    }

    private string GetPrivateStorageRoot()
    {
        var contentRoot = string.IsNullOrWhiteSpace(_environment.ContentRootPath)
            ? AppContext.BaseDirectory
            : _environment.ContentRootPath;
        return Path.GetFullPath(Path.Combine(contentRoot, "App_Data", "CRM_Documents"));
    }

    private string GetLegacyWebRoot()
    {
        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : _environment.WebRootPath;
        return Path.GetFullPath(Path.Combine(webRoot, "Uploads", "CRM_Documents"));
    }

    private string? TryResolvePhysicalPath(int orgId, int appId, string fileUrl)
    {
        var trimmed = fileUrl.Trim();
        if (trimmed.StartsWith("private://", StringComparison.OrdinalIgnoreCase))
        {
            var rel = trimmed["private://".Length..].TrimStart('/', '\\');
            var expectedPrefix = $"org_{orgId}/app_{appId}/";
            if (!rel.Replace('\\', '/').StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var root = GetPrivateStorageRoot();
            var full = Path.GetFullPath(Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar)));
            return IsWithinRoot(full, root) ? full : null;
        }

        // Backward-compatible read for pre-existing legacy /Uploads/CRM_Documents/... records
        if (trimmed.Replace('\\', '/').StartsWith("/Uploads/CRM_Documents/", StringComparison.OrdinalIgnoreCase))
        {
            var rel = trimmed.Replace('\\', '/')["/Uploads/CRM_Documents/".Length..].TrimStart('/');
            var legacyRoot = GetLegacyWebRoot();
            var full = Path.GetFullPath(Path.Combine(legacyRoot, rel.Replace('/', Path.DirectorySeparatorChar)));
            return IsWithinRoot(full, legacyRoot) ? full : null;
        }

        return null;
    }

    private static string SanitizePathSegment(string raw, string fallback)
    {
        var cleaned = raw.Replace("..", "_").Replace('/', '_').Replace('\\', '_').Trim();
        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            cleaned = cleaned.Replace(ch, '_');
        }
        cleaned = cleaned.Trim('.', ' ');
        return string.IsNullOrWhiteSpace(cleaned) ? fallback : cleaned;
    }

    private static bool IsWithinRoot(string fullPath, string rootPath)
    {
        var normalizedRoot = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsurePathWithinRoot(string fullPath, string rootPath)
    {
        if (!IsWithinRoot(fullPath, rootPath))
            throw new ArgumentException("Invalid storage path.");
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Valid organisation ID is required.", nameof(orgId));
    }
}

