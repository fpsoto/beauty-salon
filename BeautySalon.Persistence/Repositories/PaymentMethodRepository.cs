using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class PaymentMethodRepository : EfRepository<PaymentMethod>, IPaymentMethodRepository
{
    public PaymentMethodRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PaymentMethod>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);

    public async Task<bool> HasAppointmentHistoryAsync(Guid paymentMethodId, CancellationToken cancellationToken = default) =>
        await Context.Set<Appointment>().AnyAsync(a => a.PaymentMethodId == paymentMethodId, cancellationToken);
}
