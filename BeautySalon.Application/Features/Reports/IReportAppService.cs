using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Reports;

public interface IReportAppService
{
    Task<Result<ReportSummaryDto>> GetSummaryAsync(DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default);
}
