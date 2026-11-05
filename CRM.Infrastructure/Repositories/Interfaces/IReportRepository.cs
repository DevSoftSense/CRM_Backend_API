using CRM.Domain.DTOs.Reports.Requests;
using CRM.Domain.DTOs.Reports.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface IReportRepository
{
    Task<ReportCatalogResponseDto> GetCatalogAsync(int orgId, int appId, CancellationToken cancellationToken = default);

    Task<ReportExecutionResultDto> RunReportAsync(
        int orgId,
        int appId,
        RunReportRequest request,
        CancellationToken cancellationToken = default);
}

