using Microsoft.AspNetCore.Http;

namespace CRM.Domain.DTOs.Documents.Requests;

public sealed class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public string? FileName { get; set; }
    public string? Folder { get; set; }
    public string? Description { get; set; }
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public int? OwnerId { get; set; }
}

