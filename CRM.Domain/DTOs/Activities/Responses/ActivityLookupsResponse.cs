namespace CRM.Domain.DTOs.Activities.Responses;

public sealed class ActivityLookupsResponse
{
    public IReadOnlyList<ActivityLookupItemDto> ActivityType { get; set; } = [];
    public IReadOnlyList<ActivityLookupItemDto> ActivityStatus { get; set; } = [];
    public IReadOnlyList<ActivityOwnerLookupDto> Owners { get; set; } = [];
}

public sealed class ActivityOwnerLookupDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}
