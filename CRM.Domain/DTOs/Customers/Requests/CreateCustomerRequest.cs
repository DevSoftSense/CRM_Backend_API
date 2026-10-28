namespace CRM.Domain.DTOs.Customers.Requests;

public sealed class CreateCustomerRequest
{
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string PhoneMobile { get; set; } = string.Empty;
    public string? PhoneWork { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? AccountTypeId { get; set; }
    public string? PartyType { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public string? Remarks { get; set; }
}
