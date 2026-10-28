namespace CRM.Domain.DTOs.Owners.Requests;

public sealed class GetOwnersRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
