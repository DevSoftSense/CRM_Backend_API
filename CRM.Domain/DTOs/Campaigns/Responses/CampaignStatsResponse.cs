namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class CampaignStatsResponse
{
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int CompletedCampaigns { get; set; }
    public int PlannedCampaigns { get; set; }
    public decimal TotalBudget { get; set; }
    public int TotalLeadsGenerated { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public decimal TotalWonRevenue { get; set; }
    public decimal OverallRoi { get; set; }
    public IReadOnlyList<CampaignChannelStatDto> ByChannel { get; set; } = [];
}

public sealed class CampaignChannelStatDto
{
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelCode { get; set; } = string.Empty;
    public int CampaignCount { get; set; }
    public decimal TotalBudget { get; set; }
    public int LeadsCount { get; set; }
    public decimal WonRevenue { get; set; }
}

