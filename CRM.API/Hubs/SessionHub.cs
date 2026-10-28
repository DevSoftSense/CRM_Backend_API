using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Hubs;

/// <summary>
/// Real-time session channel — logout/password change pushes ForceLogout to other browsers.
/// </summary>
[Authorize]
public sealed class SessionHub : Hub
{
    public static string UserGroup(int orgId, int userId) => $"crm-user:{orgId}:{userId}";

    public override async Task OnConnectedAsync()
    {
        if (TryGetOrgUser(Context.User, out var orgId, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(orgId, userId));
        }
        else
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetOrgUser(Context.User, out var orgId, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(orgId, userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static bool TryGetOrgUser(ClaimsPrincipal? user, out int orgId, out int userId)
    {
        orgId = 0;
        userId = 0;
        if (user?.Identity?.IsAuthenticated != true) return false;
        var orgRaw = user.FindFirst("org_id")?.Value;
        var userRaw = user.FindFirst("user_id")?.Value;
        return int.TryParse(orgRaw, out orgId) && int.TryParse(userRaw, out userId)
               && orgId > 0 && userId > 0;
    }
}
