namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class TaskLookupItemDto
{
    public int MasterId { get; set; }
    public string MasterCode { get; set; } = string.Empty;
    public string MasterName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
