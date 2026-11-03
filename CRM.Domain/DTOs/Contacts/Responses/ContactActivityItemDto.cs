namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactActivityItemDto
{
    public int ActivityId { get; set; }
    public string? ActType { get; set; }
    public string? Subject { get; set; }
    public string? Notes { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? EmployeeName { get; set; }
    public string? Status { get; set; }
}

