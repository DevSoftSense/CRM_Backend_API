namespace CRM.Domain.DTOs.Campaigns.Responses;

public class CampaignListItemDto
{
    public long CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelCode { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? StatusCode { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerCode { get; set; }
    public string? Description { get; set; }

    public int LeadsCount { get; set; }
    public int OpportunitiesCount { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal WonRevenue { get; set; }
    public int WonCount { get; set; }
    public decimal RoiPercentage { get; set; }
    public decimal CostPerLead { get; set; }

    public int TotalCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

