using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Leads.Requests;
using CRM.Domain.DTOs.Leads.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IConfiguration _configuration;

    public LeadService(ILeadRepository leadRepository, IConfiguration configuration)
    {
        _leadRepository = leadRepository;
        _configuration = configuration;
    }

    public async Task<LeadListResponse> GetLeadsAsync(
        int orgId,
        GetLeadsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetLeadsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _leadRepository.GetLeadsAsync(orgId, appId, request, cancellationToken);
        return new LeadListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<LeadDetailDto?> GetLeadByIdAsync(
        int orgId,
        long leadId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (leadId <= 0)
            throw new ArgumentException("Lead id is required.", nameof(leadId));

        var appId = GetAppId();
        return await _leadRepository.GetLeadByIdAsync(orgId, appId, leadId, cancellationToken);
    }

    public async Task<LeadDetailDto> CreateLeadAsync(
        int orgId,
        long createdBy,
        CreateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.LeadName))
            throw new ArgumentException("Lead name is required.");
        if (string.IsNullOrWhiteSpace(request.Mobile))
            throw new ArgumentException("Mobile is required.");

        var appId = GetAppId();

        // Ensure status_id is set (fn also auto-seeds; this avoids empty UI dropdown)
        if (request.StatusId is null or <= 0)
        {
            var lookups = await _leadRepository.GetLookupsAsync(orgId, appId, cancellationToken);
            var newStatus = lookups.LeadStatus
                .FirstOrDefault(s => string.Equals(s.MasterCode, "NEW", StringComparison.OrdinalIgnoreCase));
            request.StatusId = newStatus?.MasterId
                ?? lookups.LeadStatus.FirstOrDefault()?.MasterId;
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode) && request.CurrencyCode.Trim().Length != 3)
            request.CurrencyCode = "INR";

        return await _leadRepository.CreateLeadAsync(orgId, appId, createdBy, request, cancellationToken);
    }

    public async Task<LeadDetailDto> UpdateLeadAsync(
        int orgId,
        long leadId,
        long updatedBy,
        UpdateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (leadId <= 0)
            throw new ArgumentException("Lead id is required.", nameof(leadId));
        if (string.IsNullOrWhiteSpace(request.LeadName))
            throw new ArgumentException("Lead name is required.");
        if (string.IsNullOrWhiteSpace(request.Mobile))
            throw new ArgumentException("Mobile is required.");
        if (request.StatusId <= 0)
            throw new ArgumentException("Status is required.");

        var appId = GetAppId();
        return await _leadRepository.UpdateLeadAsync(orgId, appId, leadId, updatedBy, request, cancellationToken);
    }

    public async Task<DeactivateLeadResponse> DeactivateLeadAsync(
        int orgId,
        long leadId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (leadId <= 0)
            throw new ArgumentException("Lead id is required.", nameof(leadId));

        var appId = GetAppId();
        return await _leadRepository.DeactivateLeadAsync(orgId, appId, leadId, updatedBy, cancellationToken);
    }

    public async Task<LeadLookupsResponse> GetLookupsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _leadRepository.GetLookupsAsync(orgId, appId, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
