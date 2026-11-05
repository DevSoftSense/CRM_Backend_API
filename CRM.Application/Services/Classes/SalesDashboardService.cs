using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.SalesDashboard.Requests;
using CRM.Domain.DTOs.SalesDashboard.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class SalesDashboardService : ISalesDashboardService
{
    private readonly ISalesDashboardRepository _repository;
    private readonly IConfiguration _configuration;

    public SalesDashboardService(ISalesDashboardRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<SalesDashboardResponseDto> GetSalesDashboardAsync(
        int orgId, GetSalesDashboardRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetSalesDashboardRequest();
        if (string.IsNullOrWhiteSpace(request.Period))
            request.Period = "this_month";

        return await _repository.GetSalesDashboardAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<SalesDashboardLookupsResponseDto> GetLookupsAsync(
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
