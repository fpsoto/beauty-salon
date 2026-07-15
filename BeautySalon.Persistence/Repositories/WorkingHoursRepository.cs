using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class WorkingHoursRepository : EfRepository<WorkingHours>, IWorkingHoursRepository
{
    public WorkingHoursRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<WorkingHours>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(w => w.ProfessionalId == professionalId).ToListAsync(cancellationToken);
}
