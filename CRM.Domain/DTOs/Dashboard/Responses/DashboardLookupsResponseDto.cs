using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Dashboard.Responses;

public sealed class DashboardLookupsResponseDto
{
    [JsonPropertyName("owners")]
    public List<DashboardOwnerLookupDto> Owners { get; set; } = new();

    [JsonPropertyName("periods")]
    public List<DashboardPeriodOptionDto> Periods { get; set; } = new();
}

public sealed class DashboardOwnerLookupDto
{
    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public sealed class DashboardPeriodOptionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

