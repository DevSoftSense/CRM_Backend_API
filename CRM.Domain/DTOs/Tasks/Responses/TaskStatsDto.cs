namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskStatsDto
{
    public int TotalTasks { get; set; }
    public int ToDoTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<TaskDistributionDto> Distribution { get; set; } = [];
    public List<PendingTaskItemDto> PendingTasks { get; set; } = [];
}

public sealed class TaskDistributionDto
{
    public string Label { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public sealed class PendingTaskItemDto
{
    public int TaskId { get; set; }
    public string? TaskCode { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? ContactPerson { get; set; }
    public string? TaskType { get; set; }
    public string? Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DueLabel { get; set; }
    public bool IsOverdue { get; set; }
}

