using CRM.Domain.DTOs.SalesDashboard.Requests;
using CRM.Domain.DTOs.SalesDashboard.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ISalesDashboardService
{
    Task<SalesDashboardResponseDto> GetSalesDashboardAsync(
        int orgId,
        GetSalesDashboardRequest request,
        CancellationToken cancellationToken = default);

    Task<SalesDashboardLookupsResponseDto> GetLookupsAsync(
        int orgId,
        CancellationToken cancellationToken = default);
}
