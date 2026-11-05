using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Reports.Requests;
using CRM.Domain.DTOs.Reports.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IConfiguration _configuration;

    public ReportService(IReportRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<ReportCatalogResponseDto> GetCatalogAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _repository.GetCatalogAsync(orgId, GetAppId(), cancellationToken);
    }

    public async Task<ReportExecutionResultDto> RunReportAsync(
        int orgId,
        RunReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ReportCode))
            throw new ArgumentException("Report code is required.");

        return await _repository.RunReportAsync(orgId, GetAppId(), request, cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}

