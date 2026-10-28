namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class ActivityLookupsResponse
{
    public IReadOnlyList<ActivityLookupItemDto> ActivityType { get; set; } = [];
    public IReadOnlyList<ActivityLookupItemDto> ActivityStatus { get; set; } = [];
}
