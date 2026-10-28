namespace CRM.Domain.DTOs.Owners.Responses;

public sealed class OwnerListItemDto
{
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? OwnerCode { get; set; }
    public int? BranchId { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset? CreatedOn { get; set; }
    public DateTimeOffset? UpdatedOn { get; set; }
    public long TotalCount { get; set; }
}
