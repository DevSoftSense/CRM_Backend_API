namespace CRM.Domain.DTOs.Timeline.Requests;

public sealed class GetTimelineRequest
{
    public string EntityType { get; set; } = "CUSTOMER";
    public long EntityId { get; set; }
    public string InteractionType { get; set; } = "ALL";
    public int? OwnerId { get; set; }
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}

