using CRM.Domain.DTOs.Documents.Requests;
using CRM.Domain.DTOs.Documents.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IDocumentService
{
    Task<DocumentListResponse> GetDocumentsAsync(int orgId, GetDocumentsRequest request, CancellationToken cancellationToken = default);
    Task<DocumentStatsDto> GetStatsAsync(int orgId, CancellationToken cancellationToken = default);
    Task<DocumentLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto?> GetByIdAsync(int orgId, long documentId, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> UploadDocumentAsync(int orgId, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<bool> CreateFolderAsync(int orgId, string folderName, CancellationToken cancellationToken = default);
    Task<DeactivateDocumentResponse> DeactivateDocumentAsync(int orgId, long documentId, CancellationToken cancellationToken = default);
    Task<(string PhysicalPath, string FileName, string ContentType)?> GetFileDownloadAsync(int orgId, long documentId, CancellationToken cancellationToken = default);
}

