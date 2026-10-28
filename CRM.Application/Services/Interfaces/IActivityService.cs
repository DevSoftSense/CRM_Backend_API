using CRM.Domain.DTOs.Activities.Requests;
using CRM.Domain.DTOs.Activities.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IActivityService
{
    Task<ActivityListResponse> GetActivitiesAsync(int orgId, GetActivitiesRequest request, CancellationToken cancellationToken = default);
    Task<ActivityListResponse> GetCalendarAsync(int orgId, GetActivitiesRequest request, CancellationToken cancellationToken = default);
    Task<ActivityDetailDto?> GetActivityByIdAsync(int orgId, int activityId, CancellationToken cancellationToken = default);
    Task<ActivityDetailDto> CreateActivityAsync(int orgId, CreateActivityRequest request, CancellationToken cancellationToken = default);
    Task<ActivityDetailDto> UpdateActivityAsync(int orgId, int activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default);
    Task<ActivityDetailDto> CompleteActivityAsync(int orgId, int activityId, DateTime? endTime, CancellationToken cancellationToken = default);
    Task<DeactivateActivityResponse> DeactivateActivityAsync(int orgId, int activityId, CancellationToken cancellationToken = default);
    Task<ActivityLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
}
