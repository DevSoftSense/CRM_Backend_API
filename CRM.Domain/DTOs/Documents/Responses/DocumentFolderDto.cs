using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentFolderDto
{
    [JsonPropertyName("folder_id")]
    public int FolderId { get; set; }

    [JsonPropertyName("folder_name")]
    public string FolderName { get; set; } = string.Empty;

    [JsonPropertyName("folder_code")]
    public string FolderCode { get; set; } = string.Empty;

    [JsonPropertyName("doc_count")]
    public int DocCount { get; set; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }
}

