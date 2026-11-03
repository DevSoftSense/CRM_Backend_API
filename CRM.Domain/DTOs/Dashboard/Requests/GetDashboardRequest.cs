namespace CRM.Domain.DTOs.Dashboard.Requests;

public sealed class GetDashboardRequest
{
    public string Period { get; set; } = "this_month";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? OwnerEmployeeId { get; set; }
}

