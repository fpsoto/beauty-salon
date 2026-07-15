using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class AppointmentRepository : EfRepository<Appointment>, IAppointmentRepository
{
    private static readonly AppointmentStatus[] ActiveStates =
    [
        AppointmentStatus.Booked,
        AppointmentStatus.Confirmed,
        AppointmentStatus.InProgress
    ];

    public AppointmentRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(a => a.Client)
            .Include(a => a.ServiceItems)
            .Include(a => a.PaymentMethod)
            .Where(a => a.ProfessionalId == professionalId && a.Date >= from && a.Date <= to)
            .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(a => a.ServiceItems)
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasOverlapAsync(
        Guid professionalId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(a =>
            a.ProfessionalId == professionalId &&
            a.Date == date &&
            ActiveStates.Contains(a.Status) &&
            (excludeAppointmentId == null || a.Id != excludeAppointmentId) &&
            a.StartTime < endTime && startTime < a.EndTime,
            cancellationToken);
}
