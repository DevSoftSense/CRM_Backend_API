using CRM.API.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Auth.Requests;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// SoftOnCloud Auth API v2.5 — CRM product login (same everyday flow as Appointment).
/// <para>
/// POST login → SoftOnCloud login + resolve-context(productId = CRM AppId 28 from config).
/// App-id is never taken from the browser. JWT is then used on CRM product APIs.
/// </para>
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthLoginController : ControllerBase
{
    private readonly ISoftOnCloudAuthService _authService;
    private readonly SessionRevocationStore _revocationStore;
    private readonly ILogger<AuthLoginController> _logger;

    public AuthLoginController(
        ISoftOnCloudAuthService authService,
        SessionRevocationStore revocationStore,
        ILogger<AuthLoginController> logger)
    {
        _authService = authService;
        _revocationStore = revocationStore;
        _logger = logger;
    }

    /// <summary>
    /// SoftOnCloud everyday product open:
    /// 1) POST SoftOnCloud /api/auth/login
    /// 2) GET SoftOnCloud /api/auth/resolve-context?productId={CRM AppId}
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest(new { ok = false, message = "Login body is required.", code = "VALIDATION" });

        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            if (string.Equals(result.Code, "VALIDATION", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    ok = false,
                    message = result.Message,
                    code = result.Code
                });
            }

            if (string.Equals(result.Code, "ACCESS_DENIED", StringComparison.OrdinalIgnoreCase)
                || (result is { Ok: false, AccessGranted: false } && !string.IsNullOrEmpty(result.DenialReason)))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    ok = false,
                    message = result.Message,
                    denialReason = result.DenialReason,
                    code = "ACCESS_DENIED",
                    appId = result.AppId,
                    productId = result.ProductId
                });
            }

            if (!result.Ok || string.IsNullOrWhiteSpace(result.Token))
            {
                return Unauthorized(new
                {
                    ok = false,
                    message = result.Message ?? "Unable to sign in.",
                    code = result.Code ?? "LOGIN_FAILED"
                });
            }

            // Same as Appointment UI after SoftOnCloud login: align CRM revoke watermark.
            TryAlignSessionRenew(result.Token);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRM SoftOnCloud login failed for OrgId={OrgId}", request.OrgId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                ok = false,
                message = "Authentication service unavailable. Please try again.",
                code = "AUTH_UNAVAILABLE"
            });
        }
    }

    /// <summary>
    /// SoftOnCloud GET /api/auth/resolve-context — productId always from CRM AppId config (28).
    /// Call after login or when refreshing roles/plan (Appointment does this on SoftOnCloud client).
    /// </summary>
    [Authorize]
    [HttpGet("resolve-context")]
    public async Task<IActionResult> ResolveContext(CancellationToken cancellationToken)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { code = "SESSION_ENDED", message = "Missing SoftOnCloud bearer token." });

        try
        {
            var (status, body, message) = await _authService.ResolveContextAsync(auth, cancellationToken);
            if (status == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(body.HasValue
                    ? body.Value
                    : new { code = "SESSION_ENDED", message = message ?? "Your session has ended. Please sign in again." });
            }

            if (status == StatusCodes.Status403Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, body.HasValue
                    ? body.Value
                    : new { message = message ?? "Access denied.", denialReason = "product_not_activated" });
            }

            if (status is < 200 or >= 300 || body is null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = message ?? "Unable to authorize CRM product access."
                });
            }

            return Ok(body.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SoftOnCloud resolve-context proxy failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Unable to authorize CRM product access."
            });
        }
    }

    /// <summary>
    /// SoftOnCloud GET /api/auth/session-status — password-change / session probe (Auth guide §14.4).
    /// </summary>
    [Authorize]
    [HttpGet("session-status")]
    public async Task<IActionResult> SessionStatus(CancellationToken cancellationToken)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(new { code = "SESSION_ENDED", message = "Missing SoftOnCloud bearer token." });

        try
        {
            var (status, body) = await _authService.SessionStatusAsync(auth, cancellationToken);
            if (status == StatusCodes.Status401Unauthorized)
            {
                // Forward SoftOnCloud PASSWORD_CHANGED / SESSION_ENDED body when present.
                return Unauthorized(body.HasValue
                    ? body.Value
                    : new { code = "SESSION_ENDED", message = "Your session has ended. Please sign in again." });
            }

            if (status is < 200 or >= 300)
            {
                return StatusCode(status, body.HasValue ? body.Value : new { message = "Session probe failed." });
            }

            return Ok(body.HasValue ? body.Value : new { isValid = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SoftOnCloud session-status proxy failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Session probe unavailable."
            });
        }
    }

    /// <summary>SoftOnCloud GET /api/auth/countries (no auth).</summary>
    [AllowAnonymous]
    [HttpGet("countries")]
    public async Task<IActionResult> Countries(CancellationToken cancellationToken)
    {
        try
        {
            var list = await _authService.GetCountriesAsync(cancellationToken);
            return Ok(list ?? Array.Empty<object>());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SoftOnCloud countries failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                ok = false,
                message = "Unable to load country list."
            });
        }
    }

    /// <summary>Current JWT claims + configured CRM AppId.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] IConfiguration configuration)
    {
        return Ok(new
        {
            ok = true,
            orgId = User.FindFirst("org_id")?.Value,
            userId = User.FindFirst("user_id")?.Value,
            sessionId = User.FindFirst("session_id")?.Value,
            email = User.FindFirst("email")?.Value
                    ?? User.FindFirst("unique_name")?.Value
                    ?? User.FindFirst("sub")?.Value,
            appId = CrmAppIdHelper.RequireAppId(configuration),
            productId = CrmAppIdHelper.RequireAppId(configuration),
            product = "SoftOnCloud CRM"
        });
    }

    private void TryAlignSessionRenew(string token)
    {
        try
        {
            Request.Headers.Authorization = "Bearer " + token;
            var orgRaw = ReadJwtField(token, "org_id");
            var userRaw = ReadJwtField(token, "user_id");
            if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId)
                || orgId <= 0 || userId <= 0)
            {
                return;
            }

            var iatUnix = JwtIatHelper.ReadIssuedAtUnix(Request, User);
            if (iatUnix is null || iatUnix <= 0)
            {
                _revocationStore.ClearRevocation(orgId, userId);
                return;
            }

            _revocationStore.SetRevokeWatermark(orgId, userId, iatUnix.Value);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "session-renew align after login skipped");
        }
    }

    private static string? ReadJwtField(string jwt, string field)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(field, out var el)) return null;
            return el.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number => el.GetRawText(),
                System.Text.Json.JsonValueKind.String => el.GetString(),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}
