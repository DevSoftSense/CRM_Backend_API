using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Auth.Responses;

/// <summary>
/// CRM login result after SoftOnCloud login + resolve-context (SoftOnCloud Auth API v2.5).
/// Same everyday flow as Appointment: login → resolve-context(productId=app-id) → JWT for product APIs.
/// </summary>
public sealed class LoginResponse
{
    public bool Ok { get; set; }

    /// <summary>SoftOnCloud JWT — send as Authorization: Bearer on CRM + SoftOnCloud calls.</summary>
    public string? Token { get; set; }

    public string? RefreshToken { get; set; }
    public string? ExpiresAt { get; set; }
    public string? SessionId { get; set; }
    public bool RememberMe { get; set; }

    /// <summary>Configured CRM SoftOnCloud app-id (28) — never from the browser.</summary>
    public int AppId { get; set; }

    /// <summary>Same as AppId — SoftOnCloud query name productId.</summary>
    public int ProductId { get; set; }

    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }

    /// <summary>SoftOnCloud login user object (JSON as returned by SoftOnCloud).</summary>
    public JsonElement? User { get; set; }

    /// <summary>SoftOnCloud resolve-context payload (roles, plan, limits, …).</summary>
    public JsonElement? ProductContext { get; set; }

    public string? Message { get; set; }

    /// <summary>LOGIN_FAILED | ACCESS_DENIED | VALIDATION | AUTH_UNAVAILABLE</summary>
    public string? Code { get; set; }
}

public sealed class SoftOnCloudLoginApiResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; set; }

    [JsonPropertyName("sessionId")]
    public JsonElement? SessionId { get; set; }

    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; }

    [JsonPropertyName("user")]
    public JsonElement? User { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
