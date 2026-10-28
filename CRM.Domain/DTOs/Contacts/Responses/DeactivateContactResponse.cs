namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class DeactivateContactResponse
{
    public bool Ok { get; set; }
    public int ContactPersonId { get; set; }
    public bool Deleted { get; set; }
}
