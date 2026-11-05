using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.SalesDashboard.Responses;

public sealed class SalesDashboardLookupsResponseDto
{
    [JsonPropertyName("owners")]
    public List<SalesOwnerLookupDto> Owners { get; set; } = new();

    [JsonPropertyName("periods")]
    public List<SalesPeriodLookupDto> Periods { get; set; } = new();
}

public sealed class SalesOwnerLookupDto
{
    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }

    [JsonPropertyName("owner_code")]
    public string? OwnerCode { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
}

public sealed class SalesPeriodLookupDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

