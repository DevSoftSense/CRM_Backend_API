namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class DeactivateOpportunityResponse
{
    public bool Ok { get; set; }
    public long OpportunityId { get; set; }
    public bool IsDeleted { get; set; }
}
