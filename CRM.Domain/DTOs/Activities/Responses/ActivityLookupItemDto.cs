namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class ActivityLookupItemDto
{
    public int MasterId { get; set; }
    public string MasterCode { get; set; } = string.Empty;
    public string MasterName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
