using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Debts;

public interface IClientDebtAppService
{
    Task<Result<IReadOnlyList<ClientDebtEntryDto>>> GetHistoryAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientBalanceDto>>> GetOutstandingBalancesAsync(CancellationToken cancellationToken = default);
    Task<Result<ClientDebtEntryDto>> AddChargeAsync(CreateChargeRequest request, CancellationToken cancellationToken = default);
    Task<Result<ClientDebtEntryDto>> AddPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
}
