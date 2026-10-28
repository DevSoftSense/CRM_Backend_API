using CRM.Domain.DTOs.Owners.Requests;
using CRM.Domain.DTOs.Owners.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IOwnerService
{
    Task<OwnerListResponse> GetOwnersAsync(int orgId, GetOwnersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OwnerDropdownItemDto>> GetDropdownAsync(int orgId, string? search, int limit = 200, bool ensureSeed = true, int? userId = null, CancellationToken cancellationToken = default);
    Task<OwnerDetailDto?> GetOwnerByIdAsync(int orgId, int ownerId, CancellationToken cancellationToken = default);
    Task<OwnerDetailDto> CreateOwnerAsync(int orgId, int createdBy, CreateOwnerRequest request, CancellationToken cancellationToken = default);
    Task<OwnerDetailDto> UpdateOwnerAsync(int orgId, int ownerId, int updatedBy, UpdateOwnerRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateOwnerResponse> DeactivateOwnerAsync(int orgId, int ownerId, int updatedBy, CancellationToken cancellationToken = default);
}
