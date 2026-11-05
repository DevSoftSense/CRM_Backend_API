using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Campaigns.Requests;

public sealed class CreateCampaignRequest
{
    [Required(ErrorMessage = "Campaign name is required.")]
    [StringLength(200, ErrorMessage = "Campaign name cannot exceed 200 characters.")]
    public string CampaignName { get; set; } = string.Empty;

    public int? ChannelId { get; set; }
    public int? StatusId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Budget must be greater than or equal to 0.")]
    public decimal? Budget { get; set; }

    public string? CurrencyCode { get; set; } = "INR";
    public int? OwnerEmployeeId { get; set; }
    public string? Description { get; set; }
}

