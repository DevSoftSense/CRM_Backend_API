namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class DeactivateCustomerResponse
{
    public bool Ok { get; set; }
    public int AccountId { get; set; }
    public bool IsDeleted { get; set; }
}
