namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactOpportunityItemDto
{
    public long OpportunityId { get; set; }
    public string? Name { get; set; }
    public string? Stage { get; set; }
    public int? StageId { get; set; }
    public decimal Probability { get; set; }
    public decimal Amount { get; set; }
    public string? ExpectedCloseDate { get; set; }
}

