namespace CRM.Domain.DTOs.Owners.Responses;

public sealed class OwnerListResponse
{
    public IReadOnlyList<OwnerListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
