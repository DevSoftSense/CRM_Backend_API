using CRM.Application.Helpers;

namespace CRM.API.Helpers;

/// <summary>
/// CRM SoftOnCloud app-id (productId) — config only, never from browser.
/// Senior locked: <c>28</c>.
/// </summary>
public static class CrmAppIdHelper
{
    public static int RequireAppId(IConfiguration configuration) =>
        SoftOnCloudProductId.Require(configuration);
}
