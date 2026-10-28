using CRM.Domain.DTOs.Customers.Requests;
using CRM.Domain.DTOs.Customers.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(
        int orgId, int appId, GetCustomersRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, string? search, int limit, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId, int appId, int accountId, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto> CreateCustomerAsync(
        int orgId, int appId, int createdBy, CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto> UpdateCustomerAsync(
        int orgId, int appId, int accountId, int updatedBy, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId, int appId, int accountId, int updatedBy, CancellationToken cancellationToken = default);

    Task<CustomerLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}
