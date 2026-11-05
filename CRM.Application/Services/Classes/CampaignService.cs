using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Campaigns.Requests;
using CRM.Domain.DTOs.Campaigns.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IConfiguration _configuration;

    public CampaignService(ICampaignRepository campaignRepository, IConfiguration configuration)
    {
        _campaignRepository = campaignRepository;
        _configuration = configuration;
    }

    public async Task<CampaignListResponse> GetCampaignsAsync(
        int orgId,
        GetCampaignsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetCampaignsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _campaignRepository.GetCampaignsAsync(orgId, appId, request, cancellationToken);
        return new CampaignListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<CampaignDetailDto?> GetCampaignByIdAsync(
        int orgId,
        long campaignId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (campaignId <= 0)
            throw new ArgumentException("Campaign ID is required.", nameof(campaignId));

        var appId = GetAppId();
        return await _campaignRepository.GetCampaignByIdAsync(orgId, appId, campaignId, cancellationToken);
    }

    public async Task<CampaignDetailDto> CreateCampaignAsync(
        int orgId,
        long createdBy,
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CampaignName))
            throw new ArgumentException("Campaign name is required.", nameof(request.CampaignName));

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.");

        if (request.Budget.HasValue && request.Budget < 0)
            throw new ArgumentException("Budget cannot be negative.");

        var appId = GetAppId();
        return await _campaignRepository.CreateCampaignAsync(orgId, appId, createdBy, request, cancellationToken);
    }

    public async Task<CampaignDetailDto> UpdateCampaignAsync(
        int orgId,
        long campaignId,
        long updatedBy,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (campaignId <= 0)
            throw new ArgumentException("Campaign ID is required.", nameof(campaignId));

        ArgumentNullException.ThrowIfNull(request);

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.");

        if (request.Budget.HasValue && request.Budget < 0)
            throw new ArgumentException("Budget cannot be negative.");

        var appId = GetAppId();
        return await _campaignRepository.UpdateCampaignAsync(orgId, appId, campaignId, updatedBy, request, cancellationToken);
    }

    public async Task<DeactivateCampaignResponse> DeactivateCampaignAsync(
        int orgId,
        long campaignId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (campaignId <= 0)
            throw new ArgumentException("Campaign ID is required.", nameof(campaignId));

        var appId = GetAppId();
        return await _campaignRepository.DeactivateCampaignAsync(orgId, appId, campaignId, updatedBy, cancellationToken);
    }

    public async Task<CampaignLookupsResponse> GetLookupsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _campaignRepository.GetLookupsAsync(orgId, appId, cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignDropdownItemDto>> GetDropdownAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _campaignRepository.GetDropdownAsync(orgId, appId, cancellationToken);
    }

    public async Task<CampaignStatsResponse> GetStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _campaignRepository.GetStatsAsync(orgId, appId, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}

