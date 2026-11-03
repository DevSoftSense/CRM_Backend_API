using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentStatsDto
{
    [JsonPropertyName("total_documents")]
    public int TotalDocuments { get; set; }

    [JsonPropertyName("used_bytes")]
    public long UsedBytes { get; set; }

    [JsonPropertyName("quota_bytes")]
    public long QuotaBytes { get; set; }

    [JsonPropertyName("used_gb")]
    public double UsedGb { get; set; }

    [JsonPropertyName("quota_gb")]
    public double QuotaGb { get; set; }

    [JsonPropertyName("available_gb")]
    public double AvailableGb { get; set; }

    [JsonPropertyName("used_percentage")]
    public double UsedPercentage { get; set; }

    [JsonPropertyName("folders")]
    public IReadOnlyList<DocumentFolderDto> Folders { get; set; } = [];

    [JsonPropertyName("recent_activity")]
    public IReadOnlyList<DocumentActivityDto> RecentActivity { get; set; } = [];

    [JsonPropertyName("type_distribution")]
    public IReadOnlyList<DocumentTypeDistributionDto> TypeDistribution { get; set; } = [];
}

