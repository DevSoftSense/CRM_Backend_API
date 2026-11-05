using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Opportunities.Responses;

public sealed class PipelineBoardResponse
{
    public PipelineSummaryDto? Summary { get; set; }
    public IReadOnlyList<PipelineStageDto> Stages { get; set; } = [];

    [JsonPropertyName("total_opportunities")]
    public int? RootTotalOpportunities { get; set; }

    [JsonPropertyName("total_pipeline_value")]
    public decimal? RootTotalPipelineValue { get; set; }

    [JsonPropertyName("weighted_pipeline_value")]
    public decimal? RootWeightedPipelineValue { get; set; }
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

    [JsonPropertyName("opportunity_count")]
    public int? OpportunityCount
    {
        get => TotalCount;
        set { if (value.HasValue && TotalCount == 0) TotalCount = value.Value; }
    }

    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public IReadOnlyList<PipelineCardDto> Opportunities { get; set; } = [];

    [JsonPropertyName("cards")]
    public IReadOnlyList<PipelineCardDto>? Cards
    {
        get => Opportunities;
        set { if (value != null && Opportunities.Count == 0) Opportunities = value; }
    }
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
