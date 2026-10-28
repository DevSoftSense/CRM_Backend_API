namespace CRM.Domain.DTOs.Activities.Requests;

public sealed class UpdateActivityRequest
{
    public string Subject { get; set; } = string.Empty;
    public string? ActType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? CallNumber { get; set; }
    public string? CallNotes { get; set; }
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
}
