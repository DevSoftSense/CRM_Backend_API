using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Dashboard.Requests;
using CRM.Domain.DTOs.Dashboard.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly IConfiguration _configuration;

    public DashboardService(IDashboardRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<CrmDashboardResponseDto> GetDashboardAsync(
        int orgId, GetDashboardRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetDashboardRequest();
        if (string.IsNullOrWhiteSpace(request.Period))
            request.Period = "this_month";

        return await _repository.GetDashboardAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<DashboardLookupsResponseDto> GetLookupsAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _repository.GetLookupsAsync(orgId, GetAppId(), cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}

