using CRM.Domain.DTOs.Customers.Requests;
using CRM.Domain.DTOs.Customers.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerListResponse> GetCustomersAsync(int orgId, GetCustomersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDropdownItemDto>> GetDropdownAsync(int orgId, string? search, int limit = 200, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto?> GetCustomerByIdAsync(int orgId, int accountId, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto> CreateCustomerAsync(int orgId, int createdBy, CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto> UpdateCustomerAsync(int orgId, int accountId, int updatedBy, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateCustomerResponse> DeactivateCustomerAsync(int orgId, int accountId, int updatedBy, CancellationToken cancellationToken = default);
    Task<CustomerLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
}
