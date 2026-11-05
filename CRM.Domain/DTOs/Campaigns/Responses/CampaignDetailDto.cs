namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class CampaignDetailDto : CampaignListItemDto
{
    public string? OwnerEmail { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }

    public IReadOnlyList<CampaignLinkedLeadDto> RecentLeads { get; set; } = [];
    public IReadOnlyList<CampaignLinkedOpportunityDto> RecentOpportunities { get; set; } = [];
}

public sealed class CampaignLinkedLeadDto
{
    public long LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? StatusName { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CampaignLinkedOpportunityDto
{
    public long OpportunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? StageName { get; set; }
    public string? StageCode { get; set; }
    public decimal Probability { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

