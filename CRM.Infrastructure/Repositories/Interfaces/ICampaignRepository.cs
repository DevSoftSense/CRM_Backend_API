using CRM.Domain.DTOs.Campaigns.Requests;
using CRM.Domain.DTOs.Campaigns.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ICampaignRepository
{
    Task<IReadOnlyList<CampaignListItemDto>> GetCampaignsAsync(
        int orgId, int appId, GetCampaignsRequest request, CancellationToken cancellationToken = default);

    Task<CampaignDetailDto?> GetCampaignByIdAsync(
        int orgId, int appId, long campaignId, CancellationToken cancellationToken = default);

    Task<CampaignDetailDto> CreateCampaignAsync(
        int orgId, int appId, long createdBy, CreateCampaignRequest request, CancellationToken cancellationToken = default);

    Task<CampaignDetailDto> UpdateCampaignAsync(
        int orgId, int appId, long campaignId, long updatedBy, UpdateCampaignRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateCampaignResponse> DeactivateCampaignAsync(
        int orgId, int appId, long campaignId, long updatedBy, CancellationToken cancellationToken = default);

    Task<CampaignLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<CampaignStatsResponse> GetStatsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}

