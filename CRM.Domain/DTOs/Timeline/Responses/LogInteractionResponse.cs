using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Timeline.Responses;

public sealed class LogInteractionResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("activity_id")]
    public int ActivityId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("interaction_type")]
    public string InteractionType { get; set; } = string.Empty;
}

