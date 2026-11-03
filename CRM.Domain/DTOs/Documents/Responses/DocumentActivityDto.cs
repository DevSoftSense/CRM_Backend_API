using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentActivityDto
{
    [JsonPropertyName("activity_id")]
    public int ActivityId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("activity_type")]
    public string ActivityType { get; set; } = string.Empty;

    [JsonPropertyName("activity_time")]
    public DateTime? ActivityTime { get; set; }

    [JsonPropertyName("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [JsonPropertyName("time_label")]
    public string TimeLabel { get; set; } = string.Empty;
}

