using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<IReadOnlyList<PaymentMethod>> GetActiveAsync(CancellationToken cancellationToken = default);

    // Guards deletion: a method already used on a charged appointment must be
    // deactivated, not deleted, or historical reports would lose its label.
    Task<bool> HasAppointmentHistoryAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
}
