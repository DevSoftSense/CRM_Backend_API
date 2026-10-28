namespace CRM.Domain.DTOs.Owners.Requests;

public sealed class CreateOwnerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? OwnerCode { get; set; }
    public int? BranchId { get; set; }
}
