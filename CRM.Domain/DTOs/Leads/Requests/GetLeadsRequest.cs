namespace CRM.Domain.DTOs.Leads.Requests;

public sealed class GetLeadsRequest
{
    public string? Search { get; set; }
    public int? StatusId { get; set; }
    public int? SourceId { get; set; }
    public int? RatingId { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
