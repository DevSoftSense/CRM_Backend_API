
using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Timeline.Responses;

public sealed class TimelineItemDto
{
    [JsonPropertyName("timeline_id")]
    public string TimelineId { get; set; } = string.Empty;

    [JsonPropertyName("interaction_type")]
    public string InteractionType { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("direction")]
    public string? Direction { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("contact_person")]
    public string? ContactPerson { get; set; }

    [JsonPropertyName("contact_info")]
    public string? ContactInfo { get; set; }

    [JsonPropertyName("employee_id")]
    public int? EmployeeId { get; set; }

    [JsonPropertyName("employee_name")]
    public string? EmployeeName { get; set; }

    [JsonPropertyName("interaction_time")]
    public DateTime? InteractionTime { get; set; }

    [JsonPropertyName("date_label")]
    public string DateLabel { get; set; } = string.Empty;

    [JsonPropertyName("time_label")]
    public string TimeLabel { get; set; } = string.Empty;

    [JsonPropertyName("duration_label")]
    public string? DurationLabel { get; set; }

    [JsonPropertyName("attachments_count")]
    public int AttachmentsCount { get; set; }

    [JsonPropertyName("status_badge")]
    public string? StatusBadge { get; set; }
}
