namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class ActivityListResponse
{
    public IReadOnlyList<ActivityListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
