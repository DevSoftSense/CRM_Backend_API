namespace CRM.Domain.DTOs.Leads.Responses;

public sealed class DeactivateLeadResponse
{
    public bool Ok { get; set; }
    public long LeadId { get; set; }
    public bool IsDeleted { get; set; }
}
