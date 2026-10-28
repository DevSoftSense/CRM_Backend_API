namespace CRM.Domain.DTOs.Owners.Requests;

public sealed class UpdateOwnerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? OwnerCode { get; set; }
    public string Status { get; set; } = "active";
    public int? BranchId { get; set; }
}
