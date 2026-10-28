namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskListResponse
{
    public IReadOnlyList<TaskListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
