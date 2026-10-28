namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class OpportunityListResponse
{
    public IReadOnlyList<OpportunityListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
