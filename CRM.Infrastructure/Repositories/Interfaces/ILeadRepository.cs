using CRM.Domain.DTOs.Leads.Requests;
using CRM.Domain.DTOs.Leads.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ILeadRepository
{
    Task<IReadOnlyList<LeadListItemDto>> GetLeadsAsync(
        int orgId, int appId, GetLeadsRequest request, CancellationToken cancellationToken = default);

    Task<LeadDetailDto?> GetLeadByIdAsync(
        int orgId, int appId, long leadId, CancellationToken cancellationToken = default);

    Task<LeadDetailDto> CreateLeadAsync(
        int orgId, int appId, long createdBy, CreateLeadRequest request, CancellationToken cancellationToken = default);

    Task<LeadDetailDto> UpdateLeadAsync(
        int orgId, int appId, long leadId, long updatedBy, UpdateLeadRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateLeadResponse> DeactivateLeadAsync(
        int orgId, int appId, long leadId, long updatedBy, CancellationToken cancellationToken = default);

    Task<LeadLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}
