using CRM.API.Helpers;
using CRM.API.Services;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// One-shot logout revoke + SignalR push to other browsers (same SoftOnCloud pattern as Appointment).
/// </summary>
[Authorize]
[ApiController]
[Route("api/auth")]
public sealed class AuthSessionController : ControllerBase
{
    private readonly SessionRevocationStore _revocationStore;
    private readonly ISessionForceLogoutNotifier _forceLogoutNotifier;

    public AuthSessionController(
        SessionRevocationStore revocationStore,
        ISessionForceLogoutNotifier forceLogoutNotifier)
    {
        _revocationStore = revocationStore;
        _forceLogoutNotifier = forceLogoutNotifier;
    }

    public sealed class LogoutRequest
    {
        public string? ExceptConnectionId { get; set; }
        public string? Reason { get; set; }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? body,
        CancellationToken cancellationToken)
    {
        var orgRaw = User.FindFirst("org_id")?.Value;
        var userRaw = User.FindFirst("user_id")?.Value;
        if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId)
            || orgId <= 0 || userId <= 0)
        {
            return Ok(new { ok = true, revoked = false });
        }

        _revocationStore.RevokeUser(orgId, userId, DateTimeOffset.UtcNow);

        var reason = string.Equals(body?.Reason, "password_changed", StringComparison.OrdinalIgnoreCase)
            ? "password_changed"
            : "session_ended";

        await _forceLogoutNotifier.NotifyUserAsync(
            orgId,
            userId,
            reason,
            body?.ExceptConnectionId,
            cancellationToken);

        return Ok(new { ok = true, revoked = true });
    }

    [HttpPost("session-renew")]
    public IActionResult SessionRenew()
    {
        var orgRaw = User.FindFirst("org_id")?.Value;
        var userRaw = User.FindFirst("user_id")?.Value;
        if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId)
            || orgId <= 0 || userId <= 0)
        {
            return Ok(new { ok = true, renewed = false });
        }

        var iatUnix = JwtIatHelper.ReadIssuedAtUnix(Request, User);
        if (iatUnix is null || iatUnix <= 0)
        {
            _revocationStore.ClearRevocation(orgId, userId);
            return Ok(new { ok = true, renewed = true, cleared = true });
        }

        _revocationStore.SetRevokeWatermark(orgId, userId, iatUnix.Value);
        return Ok(new { ok = true, renewed = true });
    }
}
