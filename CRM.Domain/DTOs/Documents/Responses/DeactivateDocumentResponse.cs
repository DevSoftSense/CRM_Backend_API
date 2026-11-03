using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DeactivateDocumentResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("document_id")]
    public long DocumentId { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; }
}

