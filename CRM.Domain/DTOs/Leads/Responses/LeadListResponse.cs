namespace CRM.Domain.DTOs.Leads.Responses;

public sealed class LeadListResponse
{
    public IReadOnlyList<LeadListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
