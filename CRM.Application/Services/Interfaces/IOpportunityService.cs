using CRM.Domain.DTOs.Opportunities.Requests;
using CRM.Domain.DTOs.Opportunities.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IOpportunityService
{
    Task<OpportunityListResponse> GetOpportunitiesAsync(int orgId, GetOpportunitiesRequest request, CancellationToken cancellationToken = default);
    Task<OpportunityDetailDto?> GetOpportunityByIdAsync(int orgId, long opportunityId, CancellationToken cancellationToken = default);
    Task<OpportunityDetailDto> CreateOpportunityAsync(int orgId, long createdBy, CreateOpportunityRequest request, CancellationToken cancellationToken = default);
    Task<OpportunityDetailDto> UpdateOpportunityAsync(int orgId, long opportunityId, long updatedBy, UpdateOpportunityRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateOpportunityResponse> DeactivateOpportunityAsync(int orgId, long opportunityId, long updatedBy, CancellationToken cancellationToken = default);
    Task<OpportunityLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
    Task<PipelineBoardResponse> GetPipelineAsync(int orgId, string? search, CancellationToken cancellationToken = default);
    Task<OpportunityDetailDto> MoveStageAsync(int orgId, long opportunityId, int stageId, long updatedBy, CancellationToken cancellationToken = default);
}
