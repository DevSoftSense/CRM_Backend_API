using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Reports.Responses;

public sealed class ReportExecutionResultDto
{
    [JsonPropertyName("report_code")]
    public string ReportCode { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public JsonElement Summary { get; set; }

    [JsonPropertyName("chart_data")]
    public JsonElement ChartData { get; set; }

    [JsonPropertyName("rows")]
    public JsonElement Rows { get; set; }
}

