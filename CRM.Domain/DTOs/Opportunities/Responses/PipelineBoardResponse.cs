namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class PipelineBoardResponse
{
    public PipelineSummaryDto? Summary { get; set; }
    public IReadOnlyList<PipelineStageDto> Stages { get; set; } = [];
}

public sealed class PipelineSummaryDto
{
    public int TotalOpportunities { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public decimal WeightedValue { get; set; }
    public decimal AvgWinProbability { get; set; }
    public int WonCount { get; set; }
    public decimal WonAmount { get; set; }
    public int LostCount { get; set; }
    public decimal LostAmount { get; set; }
}

public sealed class PipelineStageDto
{
    public int StageId { get; set; }
    public string? StageCode { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public IReadOnlyList<PipelineCardDto> Opportunities { get; set; } = [];
}

public sealed class PipelineCardDto
{
    public long OpportunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? Probability { get; set; }
    public DateOnly? ExpectedCloseDate { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public int? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public string? StageCode { get; set; }
}
