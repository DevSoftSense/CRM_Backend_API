namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class OpportunityLookupsResponse
{
    public IReadOnlyList<OpportunityLookupItemDto> OpportunityStage { get; set; } = [];
    public IReadOnlyList<OpportunityLookupItemDto> OpportunityType { get; set; } = [];
    public IReadOnlyList<OpportunityLookupItemDto> ForecastCategory { get; set; } = [];
    public IReadOnlyList<OpportunityLookupItemDto> Priority { get; set; } = [];
    public IReadOnlyList<OpportunityLookupItemDto> LeadSource { get; set; } = [];
}
