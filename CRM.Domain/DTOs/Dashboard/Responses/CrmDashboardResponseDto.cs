using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Dashboard.Responses;

public sealed class CrmDashboardResponseDto
{
    [JsonPropertyName("meta")]
    public DashboardMetaDto Meta { get; set; } = new();

    [JsonPropertyName("kpis")]
    public DashboardKpisDto Kpis { get; set; } = new();

    [JsonPropertyName("sales_funnel")]
    public List<DashboardFunnelStageDto> SalesFunnel { get; set; } = new();

    [JsonPropertyName("pipeline_value")]
    public DashboardPipelineValueDto PipelineValue { get; set; } = new();

    [JsonPropertyName("lead_sources")]
    public List<DashboardLeadSourceDto> LeadSources { get; set; } = new();

    [JsonPropertyName("revenue_trend")]
    public List<DashboardRevenueTrendPointDto> RevenueTrend { get; set; } = new();

    [JsonPropertyName("todays_schedule")]
    public List<DashboardScheduleItemDto> TodaysSchedule { get; set; } = new();

    [JsonPropertyName("pending_followups")]
    public List<DashboardFollowupItemDto> PendingFollowups { get; set; } = new();

    [JsonPropertyName("hot_opportunities")]
    public List<DashboardHotOpportunityDto> HotOpportunities { get; set; } = new();

    [JsonPropertyName("recent_activities")]
    public List<DashboardRecentActivityDto> RecentActivities { get; set; } = new();

    [JsonPropertyName("team_performance")]
    public List<DashboardTeamPerformanceDto> TeamPerformance { get; set; } = new();

    [JsonPropertyName("ai_insights")]
    public List<DashboardAiInsightDto> AiInsights { get; set; } = new();

    [JsonPropertyName("notifications")]
    public DashboardNotificationsDto Notifications { get; set; } = new();

    [JsonPropertyName("quick_actions")]
    public List<DashboardQuickActionDto> QuickActions { get; set; } = new();
}

public sealed class DashboardMetaDto
{
    [JsonPropertyName("org_id")]
    public int OrgId { get; set; }

    [JsonPropertyName("app_id")]
    public int AppId { get; set; }

    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("owner_id")]
    public int? OwnerId { get; set; }

    [JsonPropertyName("comparison_label")]
    public string ComparisonLabel { get; set; } = string.Empty;
}

public sealed class DashboardKpisDto
{
    [JsonPropertyName("new_leads")]
    public KpiMetricWithGrowthDto NewLeads { get; set; } = new();

    [JsonPropertyName("open_opportunities")]
    public KpiMetricWithGrowthDto OpenOpportunities { get; set; } = new();

    [JsonPropertyName("today_followups")]
    public KpiMetricCountDto TodayFollowups { get; set; } = new();

    [JsonPropertyName("overdue_followups")]
    public KpiMetricCountDto OverdueFollowups { get; set; } = new();

    [JsonPropertyName("expected_revenue")]
    public KpiMetricRevenueDto ExpectedRevenue { get; set; } = new();

    [JsonPropertyName("closed_revenue")]
    public KpiMetricRevenueDto ClosedRevenue { get; set; } = new();

    [JsonPropertyName("conversion_rate")]
    public KpiMetricConversionDto ConversionRate { get; set; } = new();

    [JsonPropertyName("avg_deal_size")]
    public KpiMetricAverageDealDto AvgDealSize { get; set; } = new();
}

public sealed class KpiMetricWithGrowthDto
{
    [JsonPropertyName("value")]
    public long Value { get; set; }

    [JsonPropertyName("previous_value")]
    public long PreviousValue { get; set; }

    [JsonPropertyName("growth_pct")]
    public decimal GrowthPct { get; set; }

    [JsonPropertyName("is_positive")]
    public bool IsPositive { get; set; }

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class KpiMetricCountDto
{
    [JsonPropertyName("value")]
    public long Value { get; set; }

    [JsonPropertyName("is_urgent")]
    public bool? IsUrgent { get; set; }

    [JsonPropertyName("is_alert")]
    public bool? IsAlert { get; set; }

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class KpiMetricRevenueDto
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("previous_value")]
    public decimal PreviousValue { get; set; }

    [JsonPropertyName("growth_pct")]
    public decimal GrowthPct { get; set; }

    [JsonPropertyName("is_positive")]
    public bool IsPositive { get; set; }

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class KpiMetricConversionDto
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("won_count")]
    public long WonCount { get; set; }

    [JsonPropertyName("lost_count")]
    public long LostCount { get; set; }

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class KpiMetricAverageDealDto
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("sparkline")]
    public List<decimal> Sparkline { get; set; } = new();
}

public sealed class DashboardFunnelStageDto
{
    [JsonPropertyName("stage_code")]
    public string StageCode { get; set; } = string.Empty;

    [JsonPropertyName("stage_name")]
    public string StageName { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public sealed class DashboardPipelineValueDto
{
    [JsonPropertyName("total_pipeline_value")]
    public decimal TotalPipelineValue { get; set; }

    [JsonPropertyName("stages")]
    public List<DashboardPipelineStageDto> Stages { get; set; } = new();
}

public sealed class DashboardPipelineStageDto
{
    [JsonPropertyName("stage_id")]
    public int StageId { get; set; }

    [JsonPropertyName("stage_code")]
    public string StageCode { get; set; } = string.Empty;

    [JsonPropertyName("stage_name")]
    public string StageName { get; set; } = string.Empty;

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public sealed class DashboardLeadSourceDto
{
    [JsonPropertyName("source_id")]
    public int SourceId { get; set; }

    [JsonPropertyName("source_name")]
    public string SourceName { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public sealed class DashboardRevenueTrendPointDto
{
    [JsonPropertyName("month_label")]
    public string MonthLabel { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("month_number")]
    public int MonthNumber { get; set; }

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("deal_count")]
    public long DealCount { get; set; }
}

public sealed class DashboardScheduleItemDto
{
    [JsonPropertyName("activity_id")]
    public long ActivityId { get; set; }

    [JsonPropertyName("time_label")]
    public string TimeLabel { get; set; } = string.Empty;

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("activity_type")]
    public string ActivityType { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;
}

public sealed class DashboardFollowupItemDto
{
    [JsonPropertyName("task_id")]
    public long TaskId { get; set; }

    [JsonPropertyName("task_code")]
    public string TaskCode { get; set; } = string.Empty;

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("due_date")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("is_overdue")]
    public bool IsOverdue { get; set; }

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("contact_person")]
    public string ContactPerson { get; set; } = string.Empty;

    [JsonPropertyName("contact_phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [JsonPropertyName("assignee")]
    public string Assignee { get; set; } = string.Empty;
}

public sealed class DashboardHotOpportunityDto
{
    [JsonPropertyName("opportunity_id")]
    public long OpportunityId { get; set; }

    [JsonPropertyName("opportunity_name")]
    public string OpportunityName { get; set; } = string.Empty;

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("stage_name")]
    public string StageName { get; set; } = string.Empty;

    [JsonPropertyName("stage_code")]
    public string StageCode { get; set; } = string.Empty;

    [JsonPropertyName("probability")]
    public decimal Probability { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = "INR";

    [JsonPropertyName("expected_close_date")]
    public DateTime? ExpectedCloseDate { get; set; }

    [JsonPropertyName("owner_name")]
    public string OwnerName { get; set; } = string.Empty;
}

public sealed class DashboardRecentActivityDto
{
    [JsonPropertyName("activity_id")]
    public long ActivityId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("act_type")]
    public string ActType { get; set; } = string.Empty;

    [JsonPropertyName("time_label")]
    public string TimeLabel { get; set; } = string.Empty;

    [JsonPropertyName("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public sealed class DashboardTeamPerformanceDto
{
    [JsonPropertyName("employee_id")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [JsonPropertyName("leads_count")]
    public long LeadsCount { get; set; }

    [JsonPropertyName("won_count")]
    public long WonCount { get; set; }

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("target_amount")]
    public decimal TargetAmount { get; set; }

    [JsonPropertyName("target_pct")]
    public decimal TargetPct { get; set; }
}

public sealed class DashboardAiInsightDto
{
    [JsonPropertyName("insight_type")]
    public string InsightType { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("icon_color")]
    public string IconColor { get; set; } = string.Empty;

    [JsonPropertyName("icon_bg")]
    public string IconBg { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("entity_id")]
    public long? EntityId { get; set; }
}

public sealed class DashboardNotificationsDto
{
    [JsonPropertyName("todays_birthdays")]
    public long TodaysBirthdays { get; set; }

    [JsonPropertyName("pending_quotations")]
    public long PendingQuotations { get; set; }

    [JsonPropertyName("approval_pending")]
    public long ApprovalPending { get; set; }

    [JsonPropertyName("high_priority_customers")]
    public long HighPriorityCustomers { get; set; }

    [JsonPropertyName("overdue_payments")]
    public long OverduePayments { get; set; }

    [JsonPropertyName("new_website_leads")]
    public long NewWebsiteLeads { get; set; }
}

public sealed class DashboardQuickActionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("route")]
    public string Route { get; set; } = string.Empty;
}

