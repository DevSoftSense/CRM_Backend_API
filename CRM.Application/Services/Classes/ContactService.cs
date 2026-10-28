using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Contacts.Requests;
using CRM.Domain.DTOs.Contacts.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IConfiguration _configuration;

    public ContactService(IContactRepository contactRepository, IConfiguration configuration)
    {
        _contactRepository = contactRepository;
        _configuration = configuration;
    }

    public async Task<ContactListResponse> GetContactsAsync(
        int orgId, GetContactsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetContactsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;
        if (request.AccountId is <= 0) request.AccountId = null;

        var appId = GetAppId();
        var items = await _contactRepository.GetContactsAsync(orgId, appId, request, cancellationToken);
        return new ContactListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<IReadOnlyList<ContactDropdownItemDto>> GetDropdownAsync(
        int orgId, int? accountId, string? search, int limit = 200, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (limit <= 0) limit = 200;
        if (limit > 500) limit = 500;
        if (accountId is <= 0) accountId = null;
        var appId = GetAppId();
        return await _contactRepository.GetDropdownAsync(orgId, appId, accountId, search, limit, cancellationToken);
    }

    public async Task<ContactDetailDto?> GetContactByIdAsync(
        int orgId, int contactPersonId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (contactPersonId <= 0) throw new ArgumentException("Contact id is required.");
        var appId = GetAppId();
        return await _contactRepository.GetContactByIdAsync(orgId, appId, contactPersonId, cancellationToken);
    }

    public async Task<ContactDetailDto> CreateContactAsync(
        int orgId, CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (request.AccountId <= 0) throw new ArgumentException("Account is required.");
        if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(request.Mobile)) throw new ArgumentException("Mobile is required.");

        var appId = GetAppId();
        return await _contactRepository.CreateContactAsync(orgId, appId, request, cancellationToken);
    }

    public async Task<ContactDetailDto> UpdateContactAsync(
        int orgId, int contactPersonId, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (contactPersonId <= 0) throw new ArgumentException("Contact id is required.");
        if (request.AccountId <= 0) throw new ArgumentException("Account is required.");
        if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(request.Mobile)) throw new ArgumentException("Mobile is required.");

        var appId = GetAppId();
        return await _contactRepository.UpdateContactAsync(orgId, appId, contactPersonId, request, cancellationToken);
    }

    public async Task<DeactivateContactResponse> DeactivateContactAsync(
        int orgId, int contactPersonId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (contactPersonId <= 0) throw new ArgumentException("Contact id is required.");
        var appId = GetAppId();
        return await _contactRepository.DeactivateContactAsync(orgId, appId, contactPersonId, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}
