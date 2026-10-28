namespace CRM.Domain.DTOs.Tasks.Requests;

public sealed class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TaskType { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? StartDate { get; set; }
    public int? EmployeeId { get; set; }
    public string? Notes { get; set; }
}
