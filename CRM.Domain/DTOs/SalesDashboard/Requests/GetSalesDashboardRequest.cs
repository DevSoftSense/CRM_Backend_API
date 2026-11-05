namespace CRM.Domain.DTOs.SalesDashboard.Requests;

public sealed class GetSalesDashboardRequest
{
    public string Period { get; set; } = "this_month";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? OwnerEmployeeId { get; set; }
}

