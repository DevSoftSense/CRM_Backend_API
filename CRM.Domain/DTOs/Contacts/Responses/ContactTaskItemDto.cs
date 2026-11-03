namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactTaskItemDto
{
    public int TaskId { get; set; }
    public string? Title { get; set; }
    public string? TaskCode { get; set; }
    public string? DueDate { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public bool Completed { get; set; }
}

