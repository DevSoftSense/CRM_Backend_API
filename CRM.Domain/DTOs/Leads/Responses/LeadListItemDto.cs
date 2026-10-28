namespace CRM.Domain.DTOs.Leads.Responses;

public sealed class LeadListItemDto
{
    public long LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public int? RatingId { get; set; }
    public string? RatingName { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? CurrencyCode { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public long? CampaignId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long TotalCount { get; set; }
}
