using CRM.Domain.DTOs.Leads.Requests;
using CRM.Domain.DTOs.Leads.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ILeadService
{
    Task<LeadListResponse> GetLeadsAsync(int orgId, GetLeadsRequest request, CancellationToken cancellationToken = default);
    Task<LeadDetailDto?> GetLeadByIdAsync(int orgId, long leadId, CancellationToken cancellationToken = default);
    Task<LeadDetailDto> CreateLeadAsync(int orgId, long createdBy, CreateLeadRequest request, CancellationToken cancellationToken = default);
    Task<LeadDetailDto> UpdateLeadAsync(int orgId, long leadId, long updatedBy, UpdateLeadRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateLeadResponse> DeactivateLeadAsync(int orgId, long leadId, long updatedBy, CancellationToken cancellationToken = default);
    Task<LeadLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
}
