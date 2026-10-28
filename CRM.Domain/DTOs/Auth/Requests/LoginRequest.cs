namespace CRM.Domain.DTOs.Auth.Requests;

/// <summary>
/// SoftOnCloud login body — same fields as Appointment UI → SoftOnCloud POST /api/auth/login.
/// </summary>
public sealed class LoginRequest
{
    public int OrgId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
