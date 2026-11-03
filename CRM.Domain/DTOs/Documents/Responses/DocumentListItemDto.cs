using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentListItemDto
{
    [JsonPropertyName("document_id")]
    public long DocumentId { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonPropertyName("folder_name")]
    public string FolderName { get; set; } = string.Empty;

    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("file_size_label")]
    public string FileSizeLabel { get; set; } = string.Empty;

    [JsonPropertyName("file_type")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("entity_id")]
    public long EntityId { get; set; }

    [JsonPropertyName("related_to")]
    public string RelatedTo { get; set; } = string.Empty;

    [JsonPropertyName("owner_id")]
    public int? OwnerId { get; set; }

    [JsonPropertyName("owner_name")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("modified_on")]
    public DateTime? ModifiedOn { get; set; }

    [JsonPropertyName("modified_label")]
    public string ModifiedLabel { get; set; } = string.Empty;

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
}

