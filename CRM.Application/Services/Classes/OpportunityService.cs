using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Opportunities.Requests;
using CRM.Domain.DTOs.Opportunities.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IConfiguration _configuration;

    public OpportunityService(IOpportunityRepository opportunityRepository, IConfiguration configuration)
    {
        _opportunityRepository = opportunityRepository;
        _configuration = configuration;
    }

    public async Task<OpportunityListResponse> GetOpportunitiesAsync(
        int orgId,
        GetOpportunitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetOpportunitiesRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _opportunityRepository.GetOpportunitiesAsync(orgId, appId, request, cancellationToken);
        return new OpportunityListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<OpportunityDetailDto?> GetOpportunityByIdAsync(
        int orgId,
        long opportunityId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (opportunityId <= 0)
            throw new ArgumentException("Opportunity id is required.", nameof(opportunityId));

        var appId = GetAppId();
        return await _opportunityRepository.GetOpportunityByIdAsync(orgId, appId, opportunityId, cancellationToken);
    }

    public async Task<OpportunityDetailDto> CreateOpportunityAsync(
        int orgId,
        long createdBy,
        CreateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Opportunity name is required.");
        if (request.AccountId <= 0)
            throw new ArgumentException("Account ID is required.");

        NormalizeOptionalFks(request);
        NormalizeCurrency(request);

        var appId = GetAppId();

        if (request.StageId is null or <= 0)
        {
            var lookups = await _opportunityRepository.GetLookupsAsync(orgId, appId, cancellationToken);
            var prospecting = lookups.OpportunityStage
                .FirstOrDefault(s => string.Equals(s.MasterCode, "PROSPECTING", StringComparison.OrdinalIgnoreCase));
            request.StageId = prospecting?.MasterId
                ?? lookups.OpportunityStage.FirstOrDefault()?.MasterId;
        }

        if (request.Probability is < 0 or > 100)
            throw new ArgumentException("Probability must be between 0 and 100.");

        return await _opportunityRepository.CreateOpportunityAsync(orgId, appId, createdBy, request, cancellationToken);
    }

    public async Task<OpportunityDetailDto> UpdateOpportunityAsync(
        int orgId,
        long opportunityId,
        long updatedBy,
        UpdateOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (opportunityId <= 0)
            throw new ArgumentException("Opportunity id is required.", nameof(opportunityId));
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Opportunity name is required.");
        if (request.AccountId <= 0)
            throw new ArgumentException("Account ID is required.");
        if (request.StageId <= 0)
            throw new ArgumentException("Stage is required.");

        NormalizeOptionalFks(request);
        NormalizeCurrency(request);

        if (request.Probability is < 0 or > 100)
            throw new ArgumentException("Probability must be between 0 and 100.");

        var appId = GetAppId();
        return await _opportunityRepository.UpdateOpportunityAsync(
            orgId, appId, opportunityId, updatedBy, request, cancellationToken);
    }

    public async Task<DeactivateOpportunityResponse> DeactivateOpportunityAsync(
        int orgId,
        long opportunityId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (opportunityId <= 0)
            throw new ArgumentException("Opportunity id is required.", nameof(opportunityId));

        var appId = GetAppId();
        return await _opportunityRepository.DeactivateOpportunityAsync(
            orgId, appId, opportunityId, updatedBy, cancellationToken);
    }

    public async Task<OpportunityLookupsResponse> GetLookupsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _opportunityRepository.GetLookupsAsync(orgId, appId, cancellationToken);
    }

    public async Task<PipelineBoardResponse> GetPipelineAsync(
        int orgId, string? search, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _opportunityRepository.GetPipelineAsync(orgId, appId, search, cancellationToken);
    }

    public async Task<OpportunityDetailDto> MoveStageAsync(
        int orgId, long opportunityId, int stageId, long updatedBy, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (opportunityId <= 0) throw new ArgumentException("Opportunity id is required.");
        if (stageId <= 0) throw new ArgumentException("Stage id is required.");
        var appId = GetAppId();
        return await _opportunityRepository.MoveStageAsync(
            orgId, appId, opportunityId, stageId, updatedBy, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }

    /// <summary>
    /// Optional related IDs: blank / &lt;= 0 → null so PostgreSQL FK columns stay NULL.
    /// </summary>
    private static void NormalizeOptionalFks(CreateOpportunityRequest request)
    {
        if (request.LeadId is null or <= 0) request.LeadId = null;
        if (request.CampaignId is null or <= 0) request.CampaignId = null;
        if (request.PrimaryContactId is null or <= 0) request.PrimaryContactId = null;
        if (request.OpportunityTypeId is null or <= 0) request.OpportunityTypeId = null;
        if (request.SourceId is null or <= 0) request.SourceId = null;
        if (request.ForecastCategoryId is null or <= 0) request.ForecastCategoryId = null;
        if (request.PriorityId is null or <= 0) request.PriorityId = null;
        if (request.OwnerEmployeeId is null or <= 0) request.OwnerEmployeeId = null;
        if (request.BranchId is null or <= 0) request.BranchId = null;
        if (request.FiscalYearId is null or <= 0) request.FiscalYearId = null;
        if (request.StageId is null or <= 0) request.StageId = null;
    }

    private static void NormalizeOptionalFks(UpdateOpportunityRequest request)
    {
        if (request.LeadId is null or <= 0) request.LeadId = null;
        if (request.CampaignId is null or <= 0) request.CampaignId = null;
        if (request.PrimaryContactId is null or <= 0) request.PrimaryContactId = null;
        if (request.OpportunityTypeId is null or <= 0) request.OpportunityTypeId = null;
        if (request.SourceId is null or <= 0) request.SourceId = null;
        if (request.ForecastCategoryId is null or <= 0) request.ForecastCategoryId = null;
        if (request.PriorityId is null or <= 0) request.PriorityId = null;
        if (request.OwnerEmployeeId is null or <= 0) request.OwnerEmployeeId = null;
        if (request.BranchId is null or <= 0) request.BranchId = null;
        if (request.FiscalYearId is null or <= 0) request.FiscalYearId = null;
    }

    private static void NormalizeCurrency(CreateOpportunityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
            request.CurrencyCode = "INR";
        else
            request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
    }

    private static void NormalizeCurrency(UpdateOpportunityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrencyCode))
            return;
        if (request.CurrencyCode.Trim().Length != 3)
            request.CurrencyCode = "INR";
        else
            request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
    }
}
