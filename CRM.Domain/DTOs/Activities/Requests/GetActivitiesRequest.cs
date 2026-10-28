namespace CRM.Domain.DTOs.Activities.Requests;

public sealed class GetActivitiesRequest
{
    public string? Search { get; set; }
    public string? ActType { get; set; }
    public string? ActivityStatus { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? StartFrom { get; set; }
    public DateTime? StartTo { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
