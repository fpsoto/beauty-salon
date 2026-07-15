using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Payments;

public interface IPaymentMethodAppService
{
    Task<Result<IReadOnlyList<PaymentMethodDto>>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodDto>> CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodDto>> UpdateAsync(Guid paymentMethodId, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
}
