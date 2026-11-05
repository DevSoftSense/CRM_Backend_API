namespace CRM.Domain.DTOs.Campaigns.Requests;

public sealed class GetCampaignsRequest
{
    public string? Search { get; set; }
    public int? ChannelId { get; set; }
    public int? StatusId { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public DateOnly? StartFrom { get; set; }
    public DateOnly? StartTo { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}

