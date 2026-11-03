using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Timeline.Responses;

public sealed class TimelineLookupsResponse
{
    [JsonPropertyName("contacts")]
    public List<TimelineContactLookupDto> Contacts { get; set; } = new();

    [JsonPropertyName("owners")]
    public List<TimelineOwnerLookupDto> Owners { get; set; } = new();

    [JsonPropertyName("interaction_types")]
    public List<TimelineTypeLookupDto> InteractionTypes { get; set; } = new();
}

public sealed class TimelineContactLookupDto
{
    [JsonPropertyName("contact_person_id")]
    public int ContactPersonId { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("is_primary")]
    public bool IsPrimary { get; set; }
}

public sealed class TimelineOwnerLookupDto
{
    [JsonPropertyName("employee_id")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public sealed class TimelineTypeLookupDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

