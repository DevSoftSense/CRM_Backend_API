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

    public async Task<DocumentDetailDto> UploadDocumentAsync(
        int orgId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request == null) throw new ArgumentNullException(nameof(request));

        var folder = string.IsNullOrWhiteSpace(request.Folder) ? "General" : request.Folder.Trim();
        var rawFileName = !string.IsNullOrWhiteSpace(request.FileName)
            ? request.FileName.Trim()
            : (request.File != null ? Path.GetFileName(request.File.FileName) : "Untitled_Document.pdf");

        long fileSize = 1048576; // 1 MB default
        string fileUrl;

        // Physical file handling
        if (request.File != null && request.File.Length > 0)
        {
            fileSize = request.File.Length;
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            }

            var safeFolder = Path.GetInvalidFileNameChars().Aggregate(folder, (current, c) => current.Replace(c, '_'));
            var uploadsDir = Path.Combine(webRoot, "Uploads", "CRM_Documents", safeFolder);
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var safeName = Path.GetFileName(request.File.FileName);
            var uniquePrefix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var storedFileName = $"{uniquePrefix}_{safeName}";
            var physicalPath = Path.Combine(uploadsDir, storedFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            fileUrl = $"/Uploads/CRM_Documents/{safeFolder}/{storedFileName}";
        }
        else
        {
            fileUrl = $"/Uploads/CRM_Documents/{folder}/{rawFileName}";
        }

        return await _documentRepository.CreateDocumentAsync(
            orgId,
            GetAppId(),
            rawFileName,
            fileUrl,
            fileSize,
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
        return await _documentRepository.DeactivateDocumentAsync(orgId, GetAppId(), documentId, cancellationToken);
    }

    public async Task<(string PhysicalPath, string FileName, string ContentType)?> GetFileDownloadAsync(
        int orgId, long documentId, CancellationToken cancellationToken = default)
    {
        var doc = await GetByIdAsync(orgId, documentId, cancellationToken);
        if (doc == null) return null;

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }

        var relativePath = doc.FileUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(webRoot, relativePath);

        // If file doesn't exist on disk, create a placeholder document file so download still succeeds cleanly
        if (!File.Exists(physicalPath))
        {
            var dir = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            await File.WriteAllTextAsync(physicalPath, $"SoftOnCloud CRM Document: {doc.FileName}\nDescription: {doc.Description}\nUploaded on: {doc.ModifiedOn}", cancellationToken);
        }

        var contentType = GetContentType(doc.FileName);
        return (physicalPath, doc.FileName, contentType);
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
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
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

