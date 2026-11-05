namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class DeactivateCampaignResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long CampaignId { get; set; }
}

