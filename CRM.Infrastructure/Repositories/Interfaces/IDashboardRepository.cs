using CRM.Domain.DTOs.Dashboard.Requests;
using CRM.Domain.DTOs.Dashboard.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<CrmDashboardResponseDto> GetDashboardAsync(
        int orgId, int appId, GetDashboardRequest request, CancellationToken cancellationToken = default);

    Task<DashboardLookupsResponseDto> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}

