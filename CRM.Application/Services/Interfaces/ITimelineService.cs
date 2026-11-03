using CRM.Domain.DTOs.Timeline.Requests;
using CRM.Domain.DTOs.Timeline.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ITimelineService
{
    Task<TimelineSummaryDto> GetSummaryAsync(
        int orgId, string entityType, long entityId, CancellationToken cancellationToken = default);

    Task<TimelineStatsDto> GetStatsAsync(
        int orgId, string entityType, long? entityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimelineItemDto>> GetTimelineAsync(
        int orgId, GetTimelineRequest request, CancellationToken cancellationToken = default);

    Task<LogInteractionResponse> LogInteractionAsync(
        int orgId, LogInteractionRequest request, CancellationToken cancellationToken = default);

    Task<TimelineLookupsResponse> GetLookupsAsync(
        int orgId, long? entityId, CancellationToken cancellationToken = default);
}

