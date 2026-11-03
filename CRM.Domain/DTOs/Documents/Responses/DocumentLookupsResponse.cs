using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Documents.Responses;

public sealed class DocumentLookupAccountDto
{
    [JsonPropertyName("account_id")]
    public long AccountId { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;
}

public sealed class DocumentLookupOpportunityDto
{
    [JsonPropertyName("opportunity_id")]
    public long OpportunityId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class DocumentLookupLeadDto
{
    [JsonPropertyName("lead_id")]
    public long LeadId { get; set; }

    [JsonPropertyName("lead_name")]
    public string LeadName { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;
}

public sealed class DocumentLookupOwnerDto
{
    [JsonPropertyName("employee_id")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public sealed class DocumentLookupsResponse
{
    [JsonPropertyName("folders")]
    public IReadOnlyList<DocumentFolderDto> Folders { get; set; } = [];

    [JsonPropertyName("owners")]
    public IReadOnlyList<DocumentLookupOwnerDto> Owners { get; set; } = [];

    [JsonPropertyName("accounts")]
    public IReadOnlyList<DocumentLookupAccountDto> Accounts { get; set; } = [];

    [JsonPropertyName("opportunities")]
    public IReadOnlyList<DocumentLookupOpportunityDto> Opportunities { get; set; } = [];

    [JsonPropertyName("leads")]
    public IReadOnlyList<DocumentLookupLeadDto> Leads { get; set; } = [];

    [JsonPropertyName("types")]
    public IReadOnlyList<string> Types { get; set; } = [];
}

