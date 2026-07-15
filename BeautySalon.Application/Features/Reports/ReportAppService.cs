using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Reports;

public sealed class ReportAppService : IReportAppService
{
    // Clients with no completed visit in this many days show up as "inactive" - a
    // fixed business rule for now rather than a configurable setting.
    private const int InactivityThresholdDays = 60;
    private const int TopListSize = 5;

    private readonly IUnitOfWork _unitOfWork;

    public ReportAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReportSummaryDto>> GetSummaryAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var appointmentsInRange = await _unitOfWork.Appointments.GetByDateRangeAsync(from, to, professionalId, cancellationToken);
        var completed = appointmentsInRange.Where(a => a.Status == AppointmentStatus.Completed).ToList();

        var totalRevenue = completed.Sum(a => a.ChargedPrice ?? 0m);
        var averageRevenue = completed.Count > 0 ? totalRevenue / completed.Count : 0m;

        var revenueByPaymentMethod = completed
            .Where(a => a.PaymentMethod is not null)
            .GroupBy(a => a.PaymentMethod!.Name)
            .Select(g => new RevenueByPaymentMethodDto(g.Key, g.Sum(a => a.ChargedPrice ?? 0m)))
            .OrderByDescending(r => r.Amount)
            .ToList();

        var cancelledCount = appointmentsInRange.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShowCount = appointmentsInRange.Count(a => a.Status == AppointmentStatus.NoShow);

        // Client-side aggregation over the full history (not just the report's date
        // range) - "new/inactive" is about the whole client base, not just this period.
        var allClients = await _unitOfWork.Clients.GetAllAsync(cancellationToken);
        var newClientsCount = allClients.Count(c =>
        {
            var createdDate = DateOnly.FromDateTime(c.CreatedAt);
            return createdDate >= from && createdDate <= to;
        });

        var allAppointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        var lastVisitByClient = allAppointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.ClientId)
            .ToDictionary(g => g.Key, g => g.Max(a => a.Date));

        var inactivityCutoff = DateOnly.FromDateTime(DateTime.Now).AddDays(-InactivityThresholdDays);
        var inactiveClientsCount = allClients.Count(c =>
            c.IsActive && (!lastVisitByClient.TryGetValue(c.Id, out var lastVisit) || lastVisit < inactivityCutoff));

        var topClients = completed
            .GroupBy(a => a.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalSpent = g.Sum(a => a.ChargedPrice ?? 0m),
                VisitCount = g.Count()
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(TopListSize)
            .Join(allClients, x => x.ClientId, c => c.Id, (x, c) => new TopClientDto(c.Id, $"{c.Name} {c.LastName}", x.TotalSpent, x.VisitCount))
            .ToList();

        var allServices = await _unitOfWork.SalonServices.GetAllAsync(cancellationToken);
        var categoryIdByService = allServices.ToDictionary(s => s.Id, s => s.CategoryId);

        var allCategories = await _unitOfWork.ServiceCategories.GetAllAsync(cancellationToken);
        var categoryNameById = allCategories.ToDictionary(c => c.Id, c => c.Name);

        var serviceItems = appointmentsInRange
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .SelectMany(a => a.ServiceItems)
            .ToList();

        var topServices = serviceItems
            .GroupBy(i => i.ServiceId)
            .Select(g => new TopServiceDto(g.Key, g.First().SnapshotServiceName, g.Count(), g.Sum(i => i.SnapshotPrice)))
            .OrderByDescending(s => s.Count)
            .Take(TopListSize)
            .ToList();

        var topCategories = serviceItems
            .Where(i => categoryIdByService.ContainsKey(i.ServiceId))
            .GroupBy(i => categoryIdByService[i.ServiceId])
            .Select(g => new TopCategoryDto(g.Key, categoryNameById.GetValueOrDefault(g.Key, "—"), g.Count()))
            .OrderByDescending(c => c.Count)
            .Take(TopListSize)
            .ToList();

        var busiestHours = appointmentsInRange
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .GroupBy(a => a.StartTime.Hour)
            .Select(g => new HourCountDto(g.Key, g.Count()))
            .OrderByDescending(h => h.Count)
            .ToList();

        var summary = new ReportSummaryDto(
            from,
            to,
            totalRevenue,
            completed.Count,
            averageRevenue,
            newClientsCount,
            inactiveClientsCount,
            cancelledCount,
            noShowCount,
            revenueByPaymentMethod,
            topClients,
            topServices,
            topCategories,
            busiestHours);

        return Result.Success(summary);
    }
}
