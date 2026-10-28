using Microsoft.Extensions.Configuration;

namespace CRM.Application.Helpers;

/// <summary>
/// SoftOnCloud app-id / productId for CRM — config only (never from browser).
/// Senior locked: CRM = 28.
/// </summary>
public static class SoftOnCloudProductId
{
    public static int Require(IConfiguration configuration)
    {
        var id = configuration.GetValue<int?>("CRM:AppId")
                 ?? configuration.GetValue<int?>("CRM:ProductId")
                 ?? configuration.GetValue<int?>("SoftOnCloud:AppId")
                 ?? configuration.GetValue<int?>("SoftOnCloud:ProductId");

        if (id is null or <= 0)
        {
            throw new InvalidOperationException(
                "CRM SoftOnCloud app-id is not configured. Set CRM:AppId (and SoftOnCloud:ProductId) = 28.");
        }

        return id.Value;
    }
}
