namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class CustomerDropdownItemDto
{
    public int AccountId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? PartyCode { get; set; }
    public string? Status { get; set; }
}
