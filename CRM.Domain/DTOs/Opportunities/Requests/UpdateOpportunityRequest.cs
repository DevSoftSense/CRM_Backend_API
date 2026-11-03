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

    // Enhanced Fields (from Create Opportunity 360° View)
    public string? Description { get; set; }
    public decimal? Budget { get; set; }
    public string? Competitor { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? DecisionMaker { get; set; }
    public string? ReasonNeed { get; set; }
    public string? ProposalSolution { get; set; }
    public string? Notes { get; set; }
    public string? ProductsJson { get; set; }
    public DateOnly? FollowupDate { get; set; }
    public string? ReminderTime { get; set; }
    public string? ReminderNotes { get; set; }
    public string? Territory { get; set; }
    public int? PipelineId { get; set; }
}
