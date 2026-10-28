namespace CRM.Domain.DTOs.Customers.Responses;

public sealed class CustomerLookupsResponse
{
    public IReadOnlyList<CustomerAccountTypeDto> AccountTypes { get; set; } = [];
}
