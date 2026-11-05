using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Reports.Requests;

public sealed class RunReportRequest
{
    [Required]
    public string ReportCode { get; set; } = string.Empty;

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int? OwnerEmployeeId { get; set; }

    public string? ExtraFilter { get; set; }
}

