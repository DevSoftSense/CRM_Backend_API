namespace CRM.Domain.DTOs.Documents.Requests;

public sealed class GetDocumentsRequest
{
    public string? Search { get; set; }
    public string? Folder { get; set; }
    public string? FileType { get; set; }
    public int? OwnerId { get; set; }
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public string? Tab { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}

