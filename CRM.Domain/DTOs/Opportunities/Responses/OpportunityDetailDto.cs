namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class OpportunityDetailDto
{
    public long OpportunityId { get; set; }
    public string? OpportunityCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? Industry { get; set; }
    public string? Territory { get; set; }
    public string? PaymentTerms { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? PrimaryContactId { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactMobile { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactWorkPhone { get; set; }
    public long? LeadId { get; set; }
    public long? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int StageId { get; set; }
    public string? StageCode { get; set; }
    public string? StageName { get; set; }
    public int? StageSortOrder { get; set; }
    public int? DaysInStage { get; set; }
    public int? OpportunityTypeId { get; set; }
    public string? OpportunityTypeName { get; set; }
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal Probability { get; set; }
    public decimal? WeightedAmount { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateOnly? ActualCloseDate { get; set; }
    public int? ForecastCategoryId { get; set; }
    public string? ForecastCategoryName { get; set; }
    public int? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public string? NextStep { get; set; }
    public string? ClosedReason { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }

    // 360° Profile Sub-collections
    public IReadOnlyList<OpportunityContactPersonDto> KeyContacts { get; set; } = [];
    public IReadOnlyList<OpportunityActivityDto> RecentActivities { get; set; } = [];
    public IReadOnlyList<OpportunityTaskDto> NextSteps { get; set; } = [];
    public IReadOnlyList<OpportunityFileDto> Attachments { get; set; } = [];
    public IReadOnlyList<OpportunityStageProgressDto> StagesList { get; set; } = [];
}

public sealed class OpportunityContactPersonDto
{
    public int ContactPersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? WorkPhone { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class OpportunityActivityDto
{
    public int ActivityId { get; set; }
    public string? ActType { get; set; }
    public string? Reason { get; set; }
    public string? CallNotes { get; set; }
    public string? CallNumber { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? EmployeeName { get; set; }
}

public sealed class OpportunityTaskDto
{
    public int TaskId { get; set; }
    public string? TaskCode { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateOnly? FinishDate { get; set; }
    public string? AssigneeName { get; set; }
}

public sealed class OpportunityFileDto
{
    public long FileId { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTimeOffset? UploadedOn { get; set; }
}

public sealed class OpportunityStageProgressDto
{
    public int StageId { get; set; }
    public string? StageCode { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
