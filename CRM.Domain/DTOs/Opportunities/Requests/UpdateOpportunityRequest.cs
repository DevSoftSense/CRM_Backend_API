namespace CRM.Domain.DTOs.Opportunities.Requests;

public sealed class UpdateOpportunityRequest
{
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? PrimaryContactId { get; set; }
    public long? LeadId { get; set; }
    public long? CampaignId { get; set; }
    public int StageId { get; set; }
    public int? OpportunityTypeId { get; set; }
    public int? SourceId { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? Probability { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateOnly? ActualCloseDate { get; set; }
    public int? ForecastCategoryId { get; set; }
    public int? PriorityId { get; set; }
    public string? NextStep { get; set; }
    public string? ClosedReason { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
}
