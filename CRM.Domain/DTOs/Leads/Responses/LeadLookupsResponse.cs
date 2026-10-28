namespace CRM.Domain.DTOs.Leads.Responses;

public sealed class LeadLookupsResponse
{
    public IReadOnlyList<LeadLookupItemDto> LeadSource { get; set; } = [];
    public IReadOnlyList<LeadLookupItemDto> LeadStatus { get; set; } = [];
    public IReadOnlyList<LeadLookupItemDto> LeadRating { get; set; } = [];
}
