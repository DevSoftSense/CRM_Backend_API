using CRM.Domain.DTOs.Activities.Requests;
using CRM.Domain.DTOs.Activities.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IActivityRepository
{
    Task<IReadOnlyList<ActivityListItemDto>> GetActivitiesAsync(
        int orgId, int appId, GetActivitiesRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActivityListItemDto>> GetCalendarAsync(
        int orgId, int appId, GetActivitiesRequest request, CancellationToken cancellationToken = default);

    Task<ActivityDetailDto?> GetActivityByIdAsync(
        int orgId, int appId, int activityId, CancellationToken cancellationToken = default);

    Task<ActivityDetailDto> CreateActivityAsync(
        int orgId, int appId, CreateActivityRequest request, CancellationToken cancellationToken = default);

    Task<ActivityDetailDto> UpdateActivityAsync(
        int orgId, int appId, int activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default);

    Task<ActivityDetailDto> CompleteActivityAsync(
        int orgId, int appId, int activityId, DateTime? endTime, CancellationToken cancellationToken = default);

    Task<DeactivateActivityResponse> DeactivateActivityAsync(
        int orgId, int appId, int activityId, CancellationToken cancellationToken = default);

    Task<ActivityLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}
