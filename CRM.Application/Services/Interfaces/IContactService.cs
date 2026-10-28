using CRM.Domain.DTOs.Contacts.Requests;
using CRM.Domain.DTOs.Contacts.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IContactService
{
    Task<ContactListResponse> GetContactsAsync(int orgId, GetContactsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactDropdownItemDto>> GetDropdownAsync(int orgId, int? accountId, string? search, int limit = 200, CancellationToken cancellationToken = default);
    Task<ContactDetailDto?> GetContactByIdAsync(int orgId, int contactPersonId, CancellationToken cancellationToken = default);
    Task<ContactDetailDto> CreateContactAsync(int orgId, CreateContactRequest request, CancellationToken cancellationToken = default);
    Task<ContactDetailDto> UpdateContactAsync(int orgId, int contactPersonId, UpdateContactRequest request, CancellationToken cancellationToken = default);
    Task<DeactivateContactResponse> DeactivateContactAsync(int orgId, int contactPersonId, CancellationToken cancellationToken = default);
}
