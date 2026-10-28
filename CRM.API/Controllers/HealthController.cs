using CRM.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HealthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Public ping — confirms API is up (no auth).</summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        int? appId = null;
        try { appId = CrmAppIdHelper.RequireAppId(_configuration); } catch { /* not configured */ }

        return Ok(new
        {
            ok = true,
            product = "SoftOnCloud CRM",
            appIdConfigured = appId is > 0,
            appId,
            useProductConnectionDb = _configuration.GetValue<bool>("SoftOnCloud:UseProductConnectionDb")
        });
    }
}
