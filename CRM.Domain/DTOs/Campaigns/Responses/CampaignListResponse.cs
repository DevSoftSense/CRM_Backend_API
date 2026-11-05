namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class CampaignListResponse
{
    public IReadOnlyList<CampaignListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

