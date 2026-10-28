namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactListResponse
{
    public IReadOnlyList<ContactListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
