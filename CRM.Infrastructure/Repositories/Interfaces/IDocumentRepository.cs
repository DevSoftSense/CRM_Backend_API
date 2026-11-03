using CRM.Domain.DTOs.Documents.Requests;
using CRM.Domain.DTOs.Documents.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync(
        int orgId, int appId, GetDocumentsRequest request, CancellationToken cancellationToken = default);

    Task<DocumentStatsDto> GetStatsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<DocumentLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<DocumentDetailDto> GetByIdAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default);

    Task<DocumentDetailDto> CreateDocumentAsync(
        int orgId, int appId, string fileName, string fileUrl, long fileSize,
        string? folder, string? description, string? entityType, long? entityId, int? ownerId,
        CancellationToken cancellationToken = default);

    Task<bool> CreateFolderAsync(
        int orgId, int appId, string folderName, CancellationToken cancellationToken = default);

    Task<DeactivateDocumentResponse> DeactivateDocumentAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default);
}

