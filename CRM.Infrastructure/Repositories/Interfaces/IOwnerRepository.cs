using CRM.Domain.DTOs.Owners.Requests;
using CRM.Domain.DTOs.Owners.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IOwnerRepository
{
    Task<IReadOnlyList<OwnerListItemDto>> GetOwnersAsync(
        int orgId, int appId, GetOwnersRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OwnerDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, string? search, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OwnerDropdownItemDto>> EnsureSeedAsync(
        int orgId, int appId, int? createdBy, CancellationToken cancellationToken = default);

    Task<OwnerDetailDto?> GetOwnerByIdAsync(
        int orgId, int appId, int ownerId, CancellationToken cancellationToken = default);

    Task<OwnerDetailDto> CreateOwnerAsync(
        int orgId, int appId, int createdBy, CreateOwnerRequest request, CancellationToken cancellationToken = default);

    Task<OwnerDetailDto> UpdateOwnerAsync(
        int orgId, int appId, int ownerId, int updatedBy, UpdateOwnerRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateOwnerResponse> DeactivateOwnerAsync(
        int orgId, int appId, int ownerId, int updatedBy, CancellationToken cancellationToken = default);
}
