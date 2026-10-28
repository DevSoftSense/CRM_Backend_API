namespace CRM.Domain.DTOs.Contacts.Requests;

public sealed class GetContactsRequest
{
    public string? Search { get; set; }
    public int? AccountId { get; set; }
    public bool? IsPrimary { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
