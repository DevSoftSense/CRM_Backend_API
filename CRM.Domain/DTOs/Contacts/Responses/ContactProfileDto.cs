namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactProfileDto
{
    public int ContactPersonId { get; set; }
    public string? ContactCode { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? WorkPhone { get; set; }
    public bool IsPrimary { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? DateOfBirth { get; set; }
    public string? LeadSource { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public string? CustomerSince { get; set; }
    public string? LastActivity { get; set; }
    public string? NextFollowUp { get; set; }
    public string? AssistantName { get; set; }
    public string? AssistantPhone { get; set; }
    public string? PreferredContact { get; set; }
    public string? CommunicationLanguages { get; set; }
    public string? Location { get; set; }
    public List<string> Tags { get; set; } = [];
    public DateTimeOffset? CreatedOn { get; set; }
}
