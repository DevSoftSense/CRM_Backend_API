using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.SalesDashboard.Responses;

public sealed class SalesDashboardResponseDto
{
    [JsonPropertyName("meta")]
    public SalesDashboardMetaDto Meta { get; set; } = new();

    [JsonPropertyName("kpis")]
    public SalesDashboardKpisDto Kpis { get; set; } = new();

    [JsonPropertyName("revenue_trend")]
    public List<SalesRevenueTrendPointDto> RevenueTrend { get; set; } = new();

    [JsonPropertyName("deals_by_stage")]
    public SalesDealsByStageDto DealsByStage { get; set; } = new();

    [JsonPropertyName("revenue_by_source")]
    public SalesRevenueBySourceDto RevenueBySource { get; set; } = new();

    [JsonPropertyName("top_sales_reps")]
    public List<SalesTopRepDto> TopSalesReps { get; set; } = new();

    [JsonPropertyName("activities_overview")]
    public SalesActivitiesOverviewDto ActivitiesOverview { get; set; } = new();

    [JsonPropertyName("forecast_vs_target")]
    public SalesForecastVsTargetDto ForecastVsTarget { get; set; } = new();

    [JsonPropertyName("recent_won_deals")]
    public List<SalesRecentWonDealDto> RecentWonDeals { get; set; } = new();

    [JsonPropertyName("upcoming_followups")]
    public List<SalesUpcomingFollowupDto> UpcomingFollowups { get; set; } = new();
}

public sealed class SalesDashboardMetaDto
{
    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("comparison_label")]
    public string ComparisonLabel { get; set; } = string.Empty;
}

public sealed class SalesDashboardKpisDto
{
    [JsonPropertyName("total_revenue")]
    public SalesKpiCardDto TotalRevenue { get; set; } = new();

    [JsonPropertyName("won_revenue")]
    public SalesKpiCardDto WonRevenue { get; set; } = new();

    [JsonPropertyName("deals_won")]
    public SalesKpiCardDto DealsWon { get; set; } = new();

    [JsonPropertyName("open_opportunities")]
    public SalesKpiCardDto OpenOpportunities { get; set; } = new();

    [JsonPropertyName("new_leads")]
    public SalesKpiCardDto NewLeads { get; set; } = new();

    [JsonPropertyName("win_rate")]
    public SalesKpiCardDto WinRate { get; set; } = new();
}

public sealed class SalesKpiCardDto
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("growth_pct")]
    public decimal GrowthPct { get; set; }

    [JsonPropertyName("is_positive")]
    public bool IsPositive { get; set; } = true;

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class SalesRevenueTrendPointDto
{
    [JsonPropertyName("month_label")]
    public string MonthLabel { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("won_revenue")]
    public decimal WonRevenue { get; set; }
}

public sealed class SalesDealsByStageDto
{
    [JsonPropertyName("total_pipeline_value")]
    public decimal TotalPipelineValue { get; set; }

    [JsonPropertyName("stages")]
    public List<SalesStageMetricDto> Stages { get; set; } = new();
}

public sealed class SalesStageMetricDto
{
    [JsonPropertyName("stage_id")]
    public int StageId { get; set; }

    [JsonPropertyName("stage_code")]
    public string StageCode { get; set; } = string.Empty;

    [JsonPropertyName("stage_name")]
    public string StageName { get; set; } = string.Empty;

    [JsonPropertyName("deal_count")]
    public int DealCount { get; set; }

    [JsonPropertyName("stage_amount")]
    public decimal StageAmount { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#2563eb";
}

public sealed class SalesRevenueBySourceDto
{
    [JsonPropertyName("total_won_revenue")]
    public decimal TotalWonRevenue { get; set; }

    [JsonPropertyName("sources")]
    public List<SalesSourceMetricDto> Sources { get; set; } = new();
}

public sealed class SalesSourceMetricDto
{
    [JsonPropertyName("source_id")]
    public int SourceId { get; set; }

    [JsonPropertyName("source_name")]
    public string SourceName { get; set; } = string.Empty;

    [JsonPropertyName("source_code")]
    public string SourceCode { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#2563eb";
}

public sealed class SalesTopRepDto
{
    [JsonPropertyName("employee_id")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("sales_rep")]
    public string SalesRep { get; set; } = string.Empty;

    [JsonPropertyName("avatar_initials")]
    public string AvatarInitials { get; set; } = string.Empty;

    [JsonPropertyName("won_revenue")]
    public decimal WonRevenue { get; set; }

    [JsonPropertyName("deals_won")]
    public int DealsWon { get; set; }

    [JsonPropertyName("win_rate")]
    public decimal WinRate { get; set; }
}

public sealed class SalesActivitiesOverviewDto
{
    [JsonPropertyName("calls_made")]
    public SalesActivityMetricDto CallsMade { get; set; } = new();

    [JsonPropertyName("emails_sent")]
    public SalesActivityMetricDto EmailsSent { get; set; } = new();

    [JsonPropertyName("meetings")]
    public SalesActivityMetricDto Meetings { get; set; } = new();

    [JsonPropertyName("tasks_completed")]
    public SalesActivityMetricDto TasksCompleted { get; set; } = new();

    [JsonPropertyName("follow_ups")]
    public SalesActivityMetricDto FollowUps { get; set; } = new();

    [JsonPropertyName("quotes_sent")]
    public SalesActivityMetricDto QuotesSent { get; set; } = new();
}

public sealed class SalesActivityMetricDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("growth_pct")]
    public decimal GrowthPct { get; set; }
}

public sealed class SalesForecastVsTargetDto
{
    [JsonPropertyName("target_amount")]
    public decimal TargetAmount { get; set; }

    [JsonPropertyName("forecast_amount")]
    public decimal ForecastAmount { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }

    [JsonPropertyName("gap_amount")]
    public decimal GapAmount { get; set; }
}

public sealed class SalesRecentWonDealDto
{
    [JsonPropertyName("opportunity_id")]
    public long OpportunityId { get; set; }

    [JsonPropertyName("opportunity_name")]
    public string OpportunityName { get; set; } = string.Empty;

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("sales_rep")]
    public string SalesRep { get; set; } = string.Empty;

    [JsonPropertyName("close_date")]
    public string CloseDate { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Won";
}

public sealed class SalesUpcomingFollowupDto
{
    [JsonPropertyName("task_id")]
    public long TaskId { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("related_to")]
    public string RelatedTo { get; set; } = string.Empty;

    [JsonPropertyName("followup_date")]
    public string FollowupDate { get; set; } = string.Empty;

    [JsonPropertyName("followup_time")]
    public string FollowupTime { get; set; } = "10:00 AM";

    [JsonPropertyName("assigned_to")]
    public string AssignedTo { get; set; } = string.Empty;

    [JsonPropertyName("action_type")]
    public string ActionType { get; set; } = "follow_up";

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";
}

