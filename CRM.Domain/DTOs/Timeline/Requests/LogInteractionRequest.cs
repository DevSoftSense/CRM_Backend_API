namespace CRM.Domain.DTOs.Timeline.Requests;

public sealed class LogInteractionRequest
{
    public string EntityType { get; set; } = "CUSTOMER";
    public long EntityId { get; set; }
    public string InteractionType { get; set; } = "CALL";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Direction { get; set; }
    public string? ContactName { get; set; }
    public string? ContactInfo { get; set; }
    public int? DurationMinutes { get; set; }
    public int? OwnerId { get; set; }
    public DateTime? InteractionTime { get; set; }
}

