namespace CRM.Domain.DTOs.Tasks.Responses;

public sealed class DeactivateTaskResponse
{
    public bool Ok { get; set; }
    public int TaskId { get; set; }
    public bool IsDeleted { get; set; }
}
