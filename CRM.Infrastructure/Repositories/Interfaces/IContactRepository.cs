using CRM.Domain.DTOs.Contacts.Requests;
using CRM.Domain.DTOs.Contacts.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IContactRepository
{
    Task<IReadOnlyList<ContactListItemDto>> GetContactsAsync(
        int orgId, int appId, GetContactsRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactDropdownItemDto>> GetDropdownAsync(
        int orgId, int appId, int? accountId, string? search, int limit, CancellationToken cancellationToken = default);

    Task<ContactDetailDto?> GetContactByIdAsync(
        int orgId, int appId, int contactPersonId, CancellationToken cancellationToken = default);

    Task<ContactDetailDto> CreateContactAsync(
        int orgId, int appId, CreateContactRequest request, CancellationToken cancellationToken = default);

    Task<ContactDetailDto> UpdateContactAsync(
        int orgId, int appId, int contactPersonId, UpdateContactRequest request, CancellationToken cancellationToken = default);

    Task<DeactivateContactResponse> DeactivateContactAsync(
        int orgId, int appId, int contactPersonId, CancellationToken cancellationToken = default);
}
