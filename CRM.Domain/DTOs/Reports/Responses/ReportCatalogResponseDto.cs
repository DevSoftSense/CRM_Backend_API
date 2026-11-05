using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Reports.Responses;

public sealed class ReportCatalogResponseDto
{
    [JsonPropertyName("categories")]
    public List<ReportCategoryDto> Categories { get; set; } = new();

    [JsonPropertyName("reports")]
    public List<ReportMetadataDto> Reports { get; set; } = new();
}

public sealed class ReportCategoryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public sealed class ReportMetadataDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("chart_type")]
    public string ChartType { get; set; } = string.Empty;

    [JsonPropertyName("badge")]
    public string Badge { get; set; } = string.Empty;
}

