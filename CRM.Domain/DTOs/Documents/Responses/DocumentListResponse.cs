namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentListResponse
{
    public IReadOnlyList<DocumentListItemDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
}

