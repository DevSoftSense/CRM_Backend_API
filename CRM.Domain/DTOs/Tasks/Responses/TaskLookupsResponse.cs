namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskLookupsResponse
{
    public IReadOnlyList<TaskLookupItemDto> TaskStatus { get; set; } = [];
    public IReadOnlyList<TaskLookupItemDto> TaskPriority { get; set; } = [];
    public IReadOnlyList<TaskLookupItemDto> TaskType { get; set; } = [];
    public IReadOnlyList<TaskLookupOwnerDto> Owners { get; set; } = [];
    public IReadOnlyList<TaskLookupAccountDto> Accounts { get; set; } = [];
}

public sealed class TaskLookupOwnerDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public sealed class TaskLookupAccountDto
{
    public int AccountId { get; set; }
    public string? CompanyName { get; set; }
    public string? DisplayName { get; set; }
}
