namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactInvoiceItemDto
{
    public int? BillId { get; set; }
    public string? InvoiceNo { get; set; }
    public string? InvoiceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Outstanding { get; set; }
    public string? DueDate { get; set; }
    public string? Status { get; set; }
}

