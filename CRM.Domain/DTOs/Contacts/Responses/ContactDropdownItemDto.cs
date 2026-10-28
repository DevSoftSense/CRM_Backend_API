namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactDropdownItemDto
{
    public int ContactPersonId { get; set; }
    public int? AccountId { get; set; }
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public bool IsPrimary { get; set; }
}
