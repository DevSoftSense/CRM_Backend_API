using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Owners.Requests;
using CRM.Domain.DTOs.Owners.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IConfiguration _configuration;

    public OwnerService(IOwnerRepository ownerRepository, IConfiguration configuration)
    {
        _ownerRepository = ownerRepository;
        _configuration = configuration;
    }

    public async Task<OwnerListResponse> GetOwnersAsync(
        int orgId, GetOwnersRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetOwnersRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _ownerRepository.GetOwnersAsync(orgId, appId, request, cancellationToken);
        return new OwnerListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<IReadOnlyList<OwnerDropdownItemDto>> GetDropdownAsync(
        int orgId, string? search, int limit = 200, bool ensureSeed = true, int? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (limit <= 0) limit = 200;
        if (limit > 500) limit = 500;

        var appId = GetAppId();
        var items = await _ownerRepository.GetDropdownAsync(orgId, appId, search, limit, cancellationToken);
        if (ensureSeed && items.Count == 0)
            items = await _ownerRepository.EnsureSeedAsync(orgId, appId, userId, cancellationToken);
        return items;
    }

    public async Task<OwnerDetailDto?> GetOwnerByIdAsync(
        int orgId, int ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (ownerId <= 0) throw new ArgumentException("Owner id is required.");
        var appId = GetAppId();
        return await _ownerRepository.GetOwnerByIdAsync(orgId, appId, ownerId, cancellationToken);
    }

    public async Task<OwnerDetailDto> CreateOwnerAsync(
        int orgId, int createdBy, CreateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Phone)) throw new ArgumentException("Mobile / phone is required.");

        var appId = GetAppId();
        return await _ownerRepository.CreateOwnerAsync(orgId, appId, createdBy, request, cancellationToken);
    }

    public async Task<OwnerDetailDto> UpdateOwnerAsync(
        int orgId, int ownerId, int updatedBy, UpdateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (ownerId <= 0) throw new ArgumentException("Owner id is required.");
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Phone)) throw new ArgumentException("Mobile / phone is required.");
        if (string.IsNullOrWhiteSpace(request.Status)) request.Status = "active";

        var appId = GetAppId();
        return await _ownerRepository.UpdateOwnerAsync(orgId, appId, ownerId, updatedBy, request, cancellationToken);
    }

    public async Task<DeactivateOwnerResponse> DeactivateOwnerAsync(
        int orgId, int ownerId, int updatedBy, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (ownerId <= 0) throw new ArgumentException("Owner id is required.");
        var appId = GetAppId();
        return await _ownerRepository.DeactivateOwnerAsync(orgId, appId, ownerId, updatedBy, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}
