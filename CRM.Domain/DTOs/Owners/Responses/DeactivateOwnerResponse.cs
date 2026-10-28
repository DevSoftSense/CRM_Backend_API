namespace CRM.Domain.DTOs.Owners.Responses;

public sealed class DeactivateOwnerResponse
{
    public bool Ok { get; set; }
    public int OwnerId { get; set; }
    public bool IsDeleted { get; set; }
}
