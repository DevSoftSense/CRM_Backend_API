namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class DeactivateActivityResponse
{
    public bool Ok { get; set; }
    public int ActivityId { get; set; }
    public bool IsDeleted { get; set; }
}
