using CRM.Domain.DTOs.Timeline.Requests;
using CRM.Domain.DTOs.Timeline.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ITimelineRepository
{
    Task<TimelineSummaryDto> GetSummaryAsync(
        int orgId, int appId, string entityType, long entityId, CancellationToken cancellationToken = default);

    Task<TimelineStatsDto> GetStatsAsync(
        int orgId, int appId, string entityType, long? entityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimelineItemDto>> GetTimelineAsync(
        int orgId, int appId, GetTimelineRequest request, CancellationToken cancellationToken = default);

    Task<LogInteractionResponse> LogInteractionAsync(
        int orgId, int appId, LogInteractionRequest request, CancellationToken cancellationToken = default);

    Task<TimelineLookupsResponse> GetLookupsAsync(
        int orgId, int appId, long? entityId, CancellationToken cancellationToken = default);
}

