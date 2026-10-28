namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactListItemDto
{
    public int ContactPersonId { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountCompany { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? WorkPhone { get; set; }
    public bool IsPrimary { get; set; }
    public int? FiscalYearId { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public long TotalCount { get; set; }
}
