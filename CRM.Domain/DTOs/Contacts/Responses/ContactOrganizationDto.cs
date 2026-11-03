namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactOrganizationDto
{
    public int? AccountId { get; set; }
    public string? CompanyName { get; set; }
    public string? AccountName { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanySize { get; set; }
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
}

