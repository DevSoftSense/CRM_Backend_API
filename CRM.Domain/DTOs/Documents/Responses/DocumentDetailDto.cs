using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentDetailDto
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

    [JsonPropertyName("owner_id")]
    public int? OwnerId { get; set; }

    [JsonPropertyName("owner_name")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("modified_on")]
    public DateTime? ModifiedOn { get; set; }

    [JsonPropertyName("entity_id")]
    public long EntityId { get; set; }

    [JsonPropertyName("org_id")]
    public int OrgId { get; set; }

    [JsonPropertyName("app_id")]
    public int AppId { get; set; }
}

