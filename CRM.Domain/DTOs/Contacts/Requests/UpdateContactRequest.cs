namespace CRM.Domain.DTOs.Contacts.Requests;

public sealed class UpdateContactRequest
{
    public int AccountId { get; set; }
    public string? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? WorkPhone { get; set; }
    public bool IsPrimary { get; set; }
    public int? FiscalYearId { get; set; }
}
