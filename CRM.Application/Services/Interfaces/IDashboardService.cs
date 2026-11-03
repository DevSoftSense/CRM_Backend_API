using CRM.Domain.DTOs.Dashboard.Requests;
using CRM.Domain.DTOs.Dashboard.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<CrmDashboardResponseDto> GetDashboardAsync(
        int orgId, GetDashboardRequest request, CancellationToken cancellationToken = default);

    Task<DashboardLookupsResponseDto> GetLookupsAsync(
        int orgId, CancellationToken cancellationToken = default);
}

