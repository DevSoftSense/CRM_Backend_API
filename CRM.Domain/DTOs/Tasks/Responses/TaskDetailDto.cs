namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskDetailDto
{
    public int TaskId { get; set; }
    public string? Title { get; set; }
    public string? TaskCode { get; set; }
    public string? TaskType { get; set; }
    public string? TaskTypeName { get; set; }
    public string? Company { get; set; }
    public string? ContactPerson { get; set; }
    public string? Priority { get; set; }
    public string? PriorityName { get; set; }
    public string TaskStatus { get; set; } = string.Empty;
    public string? TaskStatusName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DueLabel { get; set; }
    public bool IsOverdue { get; set; }
    public int? EmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public string? Notes { get; set; }
    public string? Username { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
    public List<TaskHistoryItemDto> History { get; set; } = [];
}

public sealed class TaskHistoryItemDto
{
    public long TranId { get; set; }
    public DateTime? DateTime { get; set; }
    public string? Remarks { get; set; }
    public string? Status { get; set; }
}
