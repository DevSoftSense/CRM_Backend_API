namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactKpisDto
{
    public int ActivitiesCount { get; set; }
    public int OpportunitiesCount { get; set; }
    public int QuotesCount { get; set; }
    public int InvoicesCount { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int TicketsCount { get; set; }
    public int DocumentsCount { get; set; }
    public int NotesCount { get; set; }
}

