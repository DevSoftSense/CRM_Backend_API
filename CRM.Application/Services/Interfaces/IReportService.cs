using CRM.Domain.DTOs.Reports.Requests;
using CRM.Domain.DTOs.Reports.Responses;

namespace CRM.Application.Services.Interfaces;

public interface IReportService
{
    Task<ReportCatalogResponseDto> GetCatalogAsync(int orgId, CancellationToken cancellationToken = default);

    Task<ReportExecutionResultDto> RunReportAsync(
        int orgId,
        RunReportRequest request,
        CancellationToken cancellationToken = default);
}

