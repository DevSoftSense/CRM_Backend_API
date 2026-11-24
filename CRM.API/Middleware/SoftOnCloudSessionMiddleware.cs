using System.Text.Json;
using CRM.API.Helpers;
using CRM.Infrastructure.Data;

namespace CRM.API.Middleware;

/// <summary>
/// Blocks JWTs revoked by CRM logout / password change.
/// Cross-browser kick is SignalR ForceLogout; this is a backup on the next REST call.
/// </summary>
public sealed class SoftOnCloudSessionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;

    public SoftOnCloudSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        SessionRevocationStore revocationStore,
        SoftOnCloudSessionValidator sessionValidator)
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/auth/session-renew")
            || context.Request.Path.StartsWithSegments("/api/auth/login")
            || context.Request.Path.StartsWithSegments("/api/auth/logout")
            || context.Request.Path.StartsWithSegments("/api/auth/session-status"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (IsRevoked(context, revocationStore))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        new
                        {
                            message = "You were signed out on another browser or device. Please sign in again.",
                            code = "SESSION_ENDED"
                        },
                        JsonOptions));
                return;
            }

            // Validate bearer token authenticity & revocation against SoftOnCloud platform (cached briefly).
            var authHeader = context.Request.Headers.Authorization.ToString();
            var check = await sessionValidator.ValidateBearerAsync(authHeader, context.RequestAborted);
            if (!check.IsValid)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        new
                        {
                            message = string.IsNullOrWhiteSpace(check.Message)
                                ? "Your session has ended. Please sign in again."
                                : check.Message,
                            code = string.IsNullOrWhiteSpace(check.Code) ? "SESSION_ENDED" : check.Code
                        },
                        JsonOptions));
                return;
            }
        }

        await _next(context);
    }

    private static bool IsRevoked(HttpContext context, SessionRevocationStore store)
    {
        var user = context.User;
        if (!AuthContextHelper.TryGetAuthContext(user, out var userId, out var orgId))
            return false;
        return store.IsTokenRevoked(
            orgId,
            userId,
            JwtIatHelper.ReadIssuedAt(context.Request, user));
    }
}
