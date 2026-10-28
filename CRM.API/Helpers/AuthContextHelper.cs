using System.Security.Claims;

namespace CRM.API.Helpers;

/// <summary>
/// SoftOnCloud JWT + optional client headers for auth context beyond org_id / user_id.
/// </summary>
public static class AuthContextHelper
{
    public const string UserTypeHeader = "X-User-Type";

    public static string? GetUserType(HttpContext? httpContext)
    {
        if (httpContext is null) return null;
        return GetUserType(httpContext.User, httpContext.Request);
    }

    public static string? GetUserType(ClaimsPrincipal? user, HttpRequest? request)
    {
        if (request is not null)
        {
            if (request.Query.TryGetValue("userType", out var q) && !string.IsNullOrWhiteSpace(q))
                return q.ToString().Trim();
            if (request.Query.TryGetValue("user_type", out var q2) && !string.IsNullOrWhiteSpace(q2))
                return q2.ToString().Trim();

            var header = request.Headers[UserTypeHeader].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header))
                return header.Trim();
        }

        if (user?.Identity?.IsAuthenticated == true)
        {
            var claim = user.FindFirst("user_type")?.Value
                        ?? user.FindFirst("userType")?.Value
                        ?? user.FindFirst("identity_type")?.Value
                        ?? user.FindFirst("identityType")?.Value;
            if (!string.IsNullOrWhiteSpace(claim))
                return claim.Trim();
        }

        return null;
    }

    public static bool IsPrimeUserType(string? userType)
    {
        var t = (userType ?? "").Trim().ToLowerInvariant();
        return t is "prime" or "owner" or "org_owner";
    }

    /// <summary>JWT org_id claim → used as p_org_id for all crm.fn_* calls.</summary>
    public static int RequireOrgId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("org_id")?.Value;
        if (!int.TryParse(raw, out var orgId) || orgId <= 0)
            throw new UnauthorizedAccessException("Missing or invalid org_id in SoftOnCloud JWT.");
        return orgId;
    }

    /// <summary>JWT user_id claim.</summary>
    public static int RequireUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("user_id")?.Value;
        if (!int.TryParse(raw, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("Missing or invalid user_id in SoftOnCloud JWT.");
        return userId;
    }
}
