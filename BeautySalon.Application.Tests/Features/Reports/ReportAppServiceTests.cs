using BeautySalon.Application.Features.Reports;
using BeautySalon.Application.Features.Schedule;
using BeautySalon.Domain.Entities;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Reports;

public sealed class ReportAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly AppointmentAppService _appointmentService;
    private readonly ReportAppService _sut;
    private readonly Guid _professionalId = Guid.NewGuid();

    public ReportAppServiceTests()
    {
        var workingHoursProvider = new WorkingHoursProvider(_db.UnitOfWork);
        var availabilityChecker = new ScheduleAvailabilityChecker(_db.UnitOfWork, workingHoursProvider);

        _appointmentService = new AppointmentAppService(
            _db.UnitOfWork,
            availabilityChecker,
            new CreateAppointmentRequestValidator(),
            new RescheduleAppointmentRequestValidator(),
            new FinishAppointmentRequestValidator());

        _sut = new ReportAppService(_db.UnitOfWork);
    }

    public void Dispose() => _db.Dispose();

    // Mirrors AppointmentAppServiceTests.SeedBasicDataAsync - seeds a client, a 30-minute
    // service, a payment method, and working hours covering every day 09:00-18:00.
    private async Task<(Guid ClientId, Guid ServiceId, Guid PaymentMethodId)> SeedBasicDataAsync()
    {
        var professional = new User { Id = _professionalId, Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" };
        _db.Context.Add(professional);

        var client = new Client
        {
            Name = "Maria",
            LastName = "Gonzalez",
            Rut = BeautySalon.Domain.ValueObjects.Rut.Create("12345678-5"),
            Phone = "+56911111111"
        };
        _db.UnitOfWork.Clients.Add(client);

        var category = new ServiceCategory { Name = "Cabello", ColorHex = "#8E44AD" };
        var service = new SalonService { Name = "Corte", CategoryId = category.Id, SuggestedPrice = 10000m, DurationMinutes = 30 };
        _db.Context.Add(category);
        _db.Context.Add(service);

        var paymentMethod = new PaymentMethod { Name = "Efectivo" };
        _db.UnitOfWork.PaymentMethods.Add(paymentMethod);

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            _db.Context.Add(new WorkingHours
            {
                ProfessionalId = _professionalId,
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                IsWorkingDay = true
            });
        }

        await _db.UnitOfWork.SaveChangesAsync();

        return (client.Id, service.Id, paymentMethod.Id);
    }

    private static DateOnly NextMonday()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(1);
        return date;
    }

    [Fact]
    public async Task GetSummaryAsync_SumsRevenueOnlyFromCompletedAppointments()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var date = NextMonday();

        var completed = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.FinishAsync(new FinishAppointmentRequest(completed.Value.Id, 10000m, null, null, paymentMethodId, null));

        // Left as Booked - shouldn't count toward revenue or the completed count.
        await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(11, 0), [serviceId], null));

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        Assert.True(result.IsSuccess);
        Assert.Equal(10000m, result.Value.TotalRevenue);
        Assert.Equal(1, result.Value.CompletedAppointmentsCount);
        Assert.Equal(10000m, result.Value.AverageRevenuePerAppointment);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsCancelledAndNoShowAppointments()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        var cancelled = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.CancelAsync(cancelled.Value.Id, "Cliente canceló");

        var noShow = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(11, 0), [serviceId], null));
        await _appointmentService.MarkNoShowAsync(noShow.Value.Id);

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        Assert.Equal(1, result.Value.CancelledCount);
        Assert.Equal(1, result.Value.NoShowCount);
    }

    [Fact]
    public async Task GetSummaryAsync_GroupsRevenueByPaymentMethod()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var date = NextMonday();

        var appointment = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.FinishAsync(new FinishAppointmentRequest(appointment.Value.Id, 12000m, null, null, paymentMethodId, null));

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        var revenue = Assert.Single(result.Value.RevenueByPaymentMethod);
        Assert.Equal("Efectivo", revenue.PaymentMethodName);
        Assert.Equal(12000m, revenue.Amount);
    }

    [Fact]
    public async Task GetSummaryAsync_RanksTopClientByTotalSpent()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var date = NextMonday();

        var appointment = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.FinishAsync(new FinishAppointmentRequest(appointment.Value.Id, 15000m, null, null, paymentMethodId, null));

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        var topClient = Assert.Single(result.Value.TopClients);
        Assert.Equal(clientId, topClient.ClientId);
        Assert.Equal(15000m, topClient.TotalSpent);
        Assert.Equal(1, topClient.VisitCount);
    }

    [Fact]
    public async Task GetSummaryAsync_TracksBusiestHour()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 30), [serviceId], null));

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        var busiest = Assert.Single(result.Value.BusiestHours);
        Assert.Equal(10, busiest.Hour);
        Assert.Equal(2, busiest.Count);
    }

    [Fact]
    public async Task GetSummaryAsync_FoldsProductSaleRevenueIntoTotalsAndTopProducts()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var date = NextMonday();

        var appointment = await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));
        await _appointmentService.FinishAsync(new FinishAppointmentRequest(appointment.Value.Id, 10000m, null, null, paymentMethodId, null));

        var product = new Product { Name = "Shampoo", SalePrice = 5000m, IsActive = true };
        _db.UnitOfWork.Products.Add(product);
        await _db.UnitOfWork.SaveChangesAsync();

        _db.Context.Add(new ProductSale
        {
            ProductId = product.Id,
            SnapshotProductName = product.Name,
            SnapshotUnitPrice = 5000m,
            Quantity = 2,
            ClientId = clientId,
            PaymentMethodId = paymentMethodId,
            SaleDate = date,
            ProfessionalId = _professionalId
        });
        await _db.Context.SaveChangesAsync();

        var result = await _sut.GetSummaryAsync(date, date, _professionalId);

        Assert.Equal(20000m, result.Value.TotalRevenue);
        Assert.Equal(10000m, result.Value.ProductRevenue);

        var topProduct = Assert.Single(result.Value.TopProducts);
        Assert.Equal("Shampoo", topProduct.ProductName);
        Assert.Equal(2, topProduct.QuantitySold);
        Assert.Equal(10000m, topProduct.Revenue);

        var revenueByMethod = Assert.Single(result.Value.RevenueByPaymentMethod);
        Assert.Equal("Efectivo", revenueByMethod.PaymentMethodName);
        Assert.Equal(20000m, revenueByMethod.Amount);

        var topClient = Assert.Single(result.Value.TopClients);
        Assert.Equal(20000m, topClient.TotalSpent);
    }

    [Fact]
    public async Task GetSummaryAsync_OutsideDateRange_ExcludesAppointment()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        await _appointmentService.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));

        var result = await _sut.GetSummaryAsync(date.AddDays(1), date.AddDays(2), _professionalId);

        Assert.Empty(result.Value.BusiestHours);
        Assert.Equal(0, result.Value.CompletedAppointmentsCount);
    }
}
