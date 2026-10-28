namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class OpportunityLookupItemDto
{
    public int MasterId { get; set; }
    public string MasterCode { get; set; } = string.Empty;
    public string MasterName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
