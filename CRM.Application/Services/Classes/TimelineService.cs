using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Timeline.Requests;
using CRM.Domain.DTOs.Timeline.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class TimelineService : ITimelineService
{
    private readonly ITimelineRepository _repository;
    private readonly IConfiguration _configuration;

    public TimelineService(ITimelineRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<TimelineSummaryDto> GetSummaryAsync(
        int orgId, string entityType, long entityId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _repository.GetSummaryAsync(orgId, GetAppId(), entityType, entityId, cancellationToken);
    }

    public async Task<TimelineStatsDto> GetStatsAsync(
        int orgId, string entityType, long? entityId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _repository.GetStatsAsync(orgId, GetAppId(), entityType, entityId, cancellationToken);
    }

    public async Task<IReadOnlyList<TimelineItemDto>> GetTimelineAsync(
        int orgId, GetTimelineRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetTimelineRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        return await _repository.GetTimelineAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<LogInteractionResponse> LogInteractionAsync(
        int orgId, LogInteractionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        return await _repository.LogInteractionAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<TimelineLookupsResponse> GetLookupsAsync(
        int orgId, long? entityId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _repository.GetLookupsAsync(orgId, GetAppId(), entityId, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}

