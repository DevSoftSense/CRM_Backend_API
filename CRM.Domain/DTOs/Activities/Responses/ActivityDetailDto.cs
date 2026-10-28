namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class ActivityDetailDto
{
    public int ActivityId { get; set; }
    public string? Subject { get; set; }
    public string ActType { get; set; } = string.Empty;
    public string? ActTypeName { get; set; }
    public string ActivityStatus { get; set; } = string.Empty;
    public string? ActivityStatusName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? CallNumber { get; set; }
    public string? CallNotes { get; set; }
    public string? Username { get; set; }
    public int? EmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
}
