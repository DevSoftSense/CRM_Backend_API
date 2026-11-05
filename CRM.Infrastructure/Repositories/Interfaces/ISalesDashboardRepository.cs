using CRM.Domain.DTOs.SalesDashboard.Requests;
using CRM.Domain.DTOs.SalesDashboard.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ISalesDashboardRepository
{
    Task<SalesDashboardResponseDto> GetSalesDashboardAsync(
        int orgId,
        int appId,
        GetSalesDashboardRequest request,
        CancellationToken cancellationToken = default);

    Task<SalesDashboardLookupsResponseDto> GetLookupsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);
}

