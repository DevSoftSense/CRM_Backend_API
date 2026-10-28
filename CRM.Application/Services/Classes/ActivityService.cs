using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Activities.Requests;
using CRM.Domain.DTOs.Activities.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IConfiguration _configuration;

    public ActivityService(IActivityRepository activityRepository, IConfiguration configuration)
    {
        _activityRepository = activityRepository;
        _configuration = configuration;
    }

    public async Task<ActivityListResponse> GetActivitiesAsync(
        int orgId, GetActivitiesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetActivitiesRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var items = await _activityRepository.GetActivitiesAsync(orgId, GetAppId(), request, cancellationToken);
        return new ActivityListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ActivityListResponse> GetCalendarAsync(
        int orgId, GetActivitiesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetActivitiesRequest();
        if (request.Limit <= 0) request.Limit = 2000;
        if (request.Limit > 5000) request.Limit = 5000;
        request.Offset = 0;

        var items = await _activityRepository.GetCalendarAsync(orgId, GetAppId(), request, cancellationToken);
        return new ActivityListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ActivityDetailDto?> GetActivityByIdAsync(
        int orgId, int activityId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (activityId <= 0) throw new ArgumentException("Activity id is required.");
        return await _activityRepository.GetActivityByIdAsync(orgId, GetAppId(), activityId, cancellationToken);
    }

    public async Task<ActivityDetailDto> CreateActivityAsync(
        int orgId, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateWrite(request.Subject, request.StartTime, request.EndTime);
        if (string.IsNullOrWhiteSpace(request.ActType))
            request.ActType = "CALL";

        return await _activityRepository.CreateActivityAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<ActivityDetailDto> UpdateActivityAsync(
        int orgId, int activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (activityId <= 0) throw new ArgumentException("Activity id is required.");
        ValidateWrite(request.Subject, request.StartTime, request.EndTime);

        return await _activityRepository.UpdateActivityAsync(orgId, GetAppId(), activityId, request, cancellationToken);
    }

    public async Task<ActivityDetailDto> CompleteActivityAsync(
        int orgId, int activityId, DateTime? endTime, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (activityId <= 0) throw new ArgumentException("Activity id is required.");
        return await _activityRepository.CompleteActivityAsync(orgId, GetAppId(), activityId, endTime, cancellationToken);
    }

    public async Task<DeactivateActivityResponse> DeactivateActivityAsync(
        int orgId, int activityId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (activityId <= 0) throw new ArgumentException("Activity id is required.");
        return await _activityRepository.DeactivateActivityAsync(orgId, GetAppId(), activityId, cancellationToken);
    }

    public async Task<ActivityLookupsResponse> GetLookupsAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _activityRepository.GetLookupsAsync(orgId, GetAppId(), cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }

    private static void ValidateWrite(string subject, DateTime startTime, DateTime? endTime)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.");
        if (startTime == default)
            throw new ArgumentException("Start time is required.");
        if (endTime.HasValue && endTime.Value < startTime)
            throw new ArgumentException("End time cannot be before start time.");
    }
}
