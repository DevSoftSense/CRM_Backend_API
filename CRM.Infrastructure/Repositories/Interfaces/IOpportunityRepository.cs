using CRM.Domain.DTOs.Opportunities.Requests;
using CRM.Domain.DTOs.Opportunities.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IOpportunityRepository
{
    Task<IReadOnlyList<OpportunityListItemDto>> GetOpportunitiesAsync(
        int orgId, int appId, GetOpportunitiesRequest request, CancellationToken cancellationToken = default);

    Task<OpportunityDetailDto?> GetOpportunityByIdAsync(
        int orgId, int appId, long opportunityId, CancellationToken cancellationToken = default);

    Task<OpportunityDetailDto> CreateOpportunityAsync(
        int orgId, int appId, long createdBy, CreateOpportunityRequest request, CancellationToken cancellationToken = default);

    Task<OpportunityDetailDto> UpdateOpportunityAsync(
        int orgId, int appId, long opportunityId, long updatedBy, UpdateOpportunityRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateOpportunityResponse> DeactivateOpportunityAsync(
        int orgId, int appId, long opportunityId, long updatedBy, CancellationToken cancellationToken = default);

    Task<OpportunityLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<PipelineBoardResponse> GetPipelineAsync(
        int orgId, int appId, string? search, CancellationToken cancellationToken = default);

    Task<OpportunityDetailDto> MoveStageAsync(
        int orgId, int appId, long opportunityId, int stageId, long updatedBy, CancellationToken cancellationToken = default);
}
