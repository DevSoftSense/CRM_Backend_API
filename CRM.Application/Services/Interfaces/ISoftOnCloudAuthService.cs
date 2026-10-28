using System.Text.Json;
using CRM.Domain.DTOs.Auth.Requests;
using CRM.Domain.DTOs.Auth.Responses;

namespace CRM.Application.Services.Interfaces;

/// <summary>
/// SoftOnCloud Auth API v2.5 — HTTP only (no SoftOnCloud / V2 / V3 DB functions).
/// productId always = configured CRM AppId (28).
/// </summary>
public interface ISoftOnCloudAuthService
{
    /// <summary>
    /// Everyday product open: POST /api/auth/login then GET /api/auth/resolve-context?productId={app-id}.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// GET /api/auth/resolve-context?productId={app-id} — productId from config only.
    /// </summary>
    Task<(int StatusCode, JsonElement? Body, string? Message)> ResolveContextAsync(
        string bearerToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// GET /api/auth/session-status — password-change / session probe.
    /// </summary>
    Task<(int StatusCode, JsonElement? Body)> SessionStatusAsync(
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<object?> GetCountriesAsync(CancellationToken cancellationToken = default);
}
