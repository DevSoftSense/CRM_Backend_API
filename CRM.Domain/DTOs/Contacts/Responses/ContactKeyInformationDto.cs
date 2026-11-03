namespace CRM.Domain.DTOs.Contacts.Responses;

public sealed class ContactKeyInformationDto
{
    public string? RoleInPurchase { get; set; }
    public int? BuyingInfluence { get; set; }
    public decimal? BudgetAuthority { get; set; }
    public string? PreferredProduct { get; set; }
    public string? InterestArea { get; set; }
    public string? PainPoints { get; set; }
    public string? Notes { get; set; }
}
