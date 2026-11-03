namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactDetailDto
{
    // Backwards-compatible flattened properties
    public int ContactPersonId { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountCompany { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? WorkPhone { get; set; }
    public bool IsPrimary { get; set; }
    public int? FiscalYearId { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }

    // 360° Structured Sections matching public.fn_crm_contact
    public bool Ok { get; set; } = true;
    public ContactProfileDto? Contact { get; set; }
    public ContactOrganizationDto? Organization { get; set; }
    public ContactKeyInformationDto? KeyInformation { get; set; }
    public List<ContactOpportunityItemDto> Opportunities { get; set; } = [];
    public List<ContactInvoiceItemDto> Invoices { get; set; } = [];
    public List<ContactActivityItemDto> Activities { get; set; } = [];
    public List<ContactTaskItemDto> RecentTasks { get; set; } = [];
    public ContactKpisDto? Kpis { get; set; }
}
