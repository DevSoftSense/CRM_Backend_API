namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class OpportunityDetailDto
{
    public long OpportunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public int? PrimaryContactId { get; set; }
    public long? LeadId { get; set; }
    public long? CampaignId { get; set; }
    public int StageId { get; set; }
    public string? StageName { get; set; }
    public int? OpportunityTypeId { get; set; }
    public string? OpportunityTypeName { get; set; }
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal Probability { get; set; }
    public decimal? WeightedAmount { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateOnly? ActualCloseDate { get; set; }
    public int? ForecastCategoryId { get; set; }
    public string? ForecastCategoryName { get; set; }
    public int? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public string? NextStep { get; set; }
    public string? ClosedReason { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
}
