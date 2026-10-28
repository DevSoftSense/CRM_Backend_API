namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class CustomerListItemDto
{
    public int AccountId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneMobile { get; set; }
    public string? PhoneWork { get; set; }
    public string? PartyCode { get; set; }
    public string? PartyType { get; set; }
    public int? AccountTypeId { get; set; }
    public string? AccountTypeName { get; set; }
    public string Status { get; set; } = "active";
    public string? WebsiteUrl { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public long TotalCount { get; set; }
}
