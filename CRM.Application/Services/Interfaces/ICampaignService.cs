using CRM.Domain.DTOs.Campaigns.Requests;
using CRM.Domain.DTOs.Campaigns.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ICampaignService
{
    Task<CampaignListResponse> GetCampaignsAsync(int orgId, GetCampaignsRequest request, CancellationToken cancellationToken = default);
    Task<CampaignDetailDto?> GetCampaignByIdAsync(int orgId, long campaignId, CancellationToken cancellationToken = default);
    Task<CampaignDetailDto> CreateCampaignAsync(int orgId, long createdBy, CreateCampaignRequest request, CancellationToken cancellationToken = default);
    Task<CampaignDetailDto> UpdateCampaignAsync(int orgId, long campaignId, long updatedBy, UpdateCampaignRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateCampaignResponse> DeactivateCampaignAsync(int orgId, long campaignId, long updatedBy, CancellationToken cancellationToken = default);
    Task<CampaignLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampaignDropdownItemDto>> GetDropdownAsync(int orgId, CancellationToken cancellationToken = default);
    Task<CampaignStatsResponse> GetStatsAsync(int orgId, CancellationToken cancellationToken = default);
}

