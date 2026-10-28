using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Customers.Requests;
using CRM.Domain.DTOs.Customers.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;

    public CustomerService(ICustomerRepository customerRepository, IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
    }

    public async Task<CustomerListResponse> GetCustomersAsync(
        int orgId, GetCustomersRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetCustomersRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _customerRepository.GetCustomersAsync(orgId, appId, request, cancellationToken);
        return new CustomerListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<IReadOnlyList<CustomerDropdownItemDto>> GetDropdownAsync(
        int orgId, string? search, int limit = 200, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (limit <= 0) limit = 200;
        if (limit > 500) limit = 500;
        var appId = GetAppId();
        return await _customerRepository.GetDropdownAsync(orgId, appId, search, limit, cancellationToken);
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId, int accountId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0) throw new ArgumentException("Account id is required.");
        var appId = GetAppId();
        return await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken);
    }

    public async Task<CustomerDetailDto> CreateCustomerAsync(
        int orgId, int createdBy, CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.PhoneMobile))
            throw new ArgumentException("Mobile is required.");
        if (string.IsNullOrWhiteSpace(request.DisplayName)
            && string.IsNullOrWhiteSpace(request.CompanyName)
            && string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("Display name, company, or first name is required.");

        if (request.AccountTypeId is <= 0) request.AccountTypeId = null;

        var appId = GetAppId();
        return await _customerRepository.CreateCustomerAsync(orgId, appId, createdBy, request, cancellationToken);
    }

    public async Task<CustomerDetailDto> UpdateCustomerAsync(
        int orgId, int accountId, int updatedBy, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (accountId <= 0) throw new ArgumentException("Account id is required.");
        if (string.IsNullOrWhiteSpace(request.PhoneMobile))
            throw new ArgumentException("Mobile is required.");
        if (string.IsNullOrWhiteSpace(request.DisplayName)
            && string.IsNullOrWhiteSpace(request.CompanyName)
            && string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("Display name, company, or first name is required.");

        if (request.AccountTypeId is <= 0) request.AccountTypeId = null;
        if (string.IsNullOrWhiteSpace(request.Status)) request.Status = "active";

        var appId = GetAppId();
        return await _customerRepository.UpdateCustomerAsync(orgId, appId, accountId, updatedBy, request, cancellationToken);
    }

    public async Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId, int accountId, int updatedBy, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0) throw new ArgumentException("Account id is required.");
        var appId = GetAppId();
        return await _customerRepository.DeactivateCustomerAsync(orgId, appId, accountId, updatedBy, cancellationToken);
    }

    public async Task<CustomerLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _customerRepository.GetLookupsAsync(orgId, appId, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}
