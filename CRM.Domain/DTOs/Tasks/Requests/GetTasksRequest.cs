namespace CRM.Domain.DTOs.Tasks.Requests;

public sealed class GetTasksRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? TaskType { get; set; }
    public int? EmployeeId { get; set; }
    public DateOnly? DueFrom { get; set; }
    public DateOnly? DueTo { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
