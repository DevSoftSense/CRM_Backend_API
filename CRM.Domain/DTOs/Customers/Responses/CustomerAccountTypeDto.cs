namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class CustomerAccountTypeDto
{
    public int AccountTypeId { get; set; }
    public string PartyTypeCode { get; set; } = string.Empty;
    public string PartyTypeName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
