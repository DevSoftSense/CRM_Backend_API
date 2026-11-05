namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class CampaignDropdownItemDto
{
    public long CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string? ChannelName { get; set; }
    public string? StatusName { get; set; }
    public string? StatusCode { get; set; }
}

