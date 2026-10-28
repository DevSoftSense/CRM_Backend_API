namespace CRM.Domain.DTOs.Leads.Requests;

public sealed class CreateLeadRequest
{
    public string LeadName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Designation { get; set; }
    public string? Industry { get; set; }
    public string? Email { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public int? SourceId { get; set; }
    public int? StatusId { get; set; }
    public int? RatingId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? CurrencyCode { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public string? Description { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public long? CampaignId { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
}
