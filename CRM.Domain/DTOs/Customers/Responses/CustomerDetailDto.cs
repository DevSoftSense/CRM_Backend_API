namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class CustomerDetailDto
{
    public int AccountId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation { get; set; }
    public string? Email { get; set; }
    public string? PhoneMobile { get; set; }
    public string? PhoneMobileAlt { get; set; }
    public string? PhoneWork { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? PartyCode { get; set; }
    public string? PartyType { get; set; }
    public int? AccountTypeId { get; set; }
    public string? AccountTypeName { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public string Status { get; set; } = "active";
    public string? Remarks { get; set; }
    public DateOnly? CustomerSince { get; set; }
    public string? AcquisitionSource { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
}
