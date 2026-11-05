namespace CRM.Domain.DTOs.Campaigns.Responses;

public sealed class CampaignLookupsResponse
{
    public IReadOnlyList<CampaignLookupItemDto> Channels { get; set; } = [];
    public IReadOnlyList<CampaignLookupItemDto> Statuses { get; set; } = [];
    public IReadOnlyList<CampaignOwnerLookupDto> Owners { get; set; } = [];
    public IReadOnlyList<CampaignCurrencyLookupDto> Currencies { get; set; } = [];
}

public sealed class CampaignLookupItemDto
{
    public int MasterId { get; set; }
    public string MasterCode { get; set; } = string.Empty;
    public string MasterName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public sealed class CampaignOwnerLookupDto
{
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? OwnerCode { get; set; }
}

public sealed class CampaignCurrencyLookupDto
{
    public int MasterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

