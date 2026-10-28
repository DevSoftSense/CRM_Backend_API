namespace CRM.Domain.DTOs.Leads.Responses;

public sealed class LeadLookupItemDto
{
    public int MasterId { get; set; }
    public string MasterCode { get; set; } = string.Empty;
    public string MasterName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
