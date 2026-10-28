namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskLookupsResponse
{
    public IReadOnlyList<TaskLookupItemDto> TaskStatus { get; set; } = [];
    public IReadOnlyList<TaskLookupItemDto> TaskPriority { get; set; } = [];
    public IReadOnlyList<TaskLookupItemDto> TaskType { get; set; } = [];
}
