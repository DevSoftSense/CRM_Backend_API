namespace CRM.Domain.DTOs.Opportunities.Requests;

public sealed class GetOpportunitiesRequest
{
    public string? Search { get; set; }
    public int? StageId { get; set; }
    public int? OpportunityTypeId { get; set; }
    public int? SourceId { get; set; }
    public int? PriorityId { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public int? AccountId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
