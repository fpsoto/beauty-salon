using BeautySalon.Application.Features.Schedule;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Schedule;

public sealed class AppointmentAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly AppointmentAppService _sut;
    private readonly ScheduleAvailabilityChecker _availabilityChecker;
    private readonly Guid _professionalId = Guid.NewGuid();

    public AppointmentAppServiceTests()
    {
        var workingHoursProvider = new WorkingHoursProvider(_db.UnitOfWork);
        _availabilityChecker = new ScheduleAvailabilityChecker(_db.UnitOfWork, workingHoursProvider);

        _sut = new AppointmentAppService(
            _db.UnitOfWork,
            _availabilityChecker,
            new CreateAppointmentRequestValidator(),
            new RescheduleAppointmentRequestValidator(),
            new FinishAppointmentRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    // Seeds a client, a 30-minute service, a payment method, and working hours covering
    // every day of the week 09:00-18:00, so tests only need to pick a date/time.
    private async Task<(Guid ClientId, Guid ServiceId, Guid PaymentMethodId)> SeedBasicDataAsync()
    {
        // WorkingHours/Appointment both have a real FK to User.Id - a random ProfessionalId
        // with no backing User row fails with a FOREIGN KEY constraint (caught by this suite
        // on first run), so the professional must be a real seeded user, not just an id.
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
    public async Task CreateAsync_WithinWorkingHours_Succeeds()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        var result = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));

        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.Booked, result.Value.Status);
        Assert.Equal(new TimeOnly(10, 30), result.Value.EndTime); // 10:00 + 30 min service duration
    }

    [Fact]
    public async Task CreateAsync_OutsideWorkingHours_ReturnsConflict()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        var result = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(19, 0), [serviceId], null));

        Assert.True(result.IsFailure);
        Assert.Equal("Schedule.OutsideWorkingHours", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_OverlappingExistingAppointment_ReturnsConflict()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        await _sut.CreateAsync(new CreateAppointmentRequest(clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));

        var overlapping = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 15), [serviceId], null));

        Assert.True(overlapping.IsFailure);
        Assert.Equal("Schedule.Overlap", overlapping.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_BackToBackAppointments_DoNotOverlap()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var date = NextMonday();

        await _sut.CreateAsync(new CreateAppointmentRequest(clientId, _professionalId, date, new TimeOnly(10, 0), [serviceId], null));

        var backToBack = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, date, new TimeOnly(10, 30), [serviceId], null));

        Assert.True(backToBack.IsSuccess);
    }

    [Fact]
    public async Task ConfirmAsync_FromBooked_Succeeds()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, NextMonday(), new TimeOnly(10, 0), [serviceId], null));

        var result = await _sut.ConfirmAsync(created.Value.Id);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConfirmed_ReturnsInvalidTransition()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, NextMonday(), new TimeOnly(10, 0), [serviceId], null));
        await _sut.ConfirmAsync(created.Value.Id);

        var result = await _sut.ConfirmAsync(created.Value.Id);

        Assert.True(result.IsFailure);
        Assert.Equal("Appointment.InvalidTransition", result.Error.Code);
    }

    [Fact]
    public async Task CancelAsync_WithReason_AppendsReasonToNotes()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, NextMonday(), new TimeOnly(10, 0), [serviceId], "Cliente pidió corte"));

        var result = await _sut.CancelAsync(created.Value.Id, "Se enfermó");

        Assert.True(result.IsSuccess);

        var stored = await _db.UnitOfWork.Appointments.GetByIdAsync(created.Value.Id);
        Assert.Equal(AppointmentStatus.Cancelled, stored!.Status);
        Assert.Contains("Se enfermó", stored.Notes);
        Assert.Contains("Cliente pidió corte", stored.Notes);
    }

    [Fact]
    public async Task FinishAsync_SetsCompletedStatusAndChargedPrice()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, NextMonday(), new TimeOnly(10, 0), [serviceId], null));

        var result = await _sut.FinishAsync(new FinishAppointmentRequest(created.Value.Id, 9000m, 1000m, 500m, paymentMethodId, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.Completed, result.Value.Status);
        Assert.Equal(9000m, result.Value.ChargedPrice);
    }

    [Fact]
    public async Task RescheduleAsync_CreatesNewBookedAppointmentAndMarksOriginalAsRescheduled()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var originalDate = NextMonday();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, originalDate, new TimeOnly(10, 0), [serviceId], null));

        var newDate = originalDate.AddDays(1); // Tuesday
        var result = await _sut.RescheduleAsync(new RescheduleAppointmentRequest(created.Value.Id, newDate, new TimeOnly(11, 0)));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(created.Value.Id, result.Value.Id);
        Assert.Equal(AppointmentStatus.Booked, result.Value.Status);
        Assert.Equal(created.Value.Id, result.Value.RescheduledFromAppointmentId);

        var original = await _db.UnitOfWork.Appointments.GetByIdAsync(created.Value.Id);
        Assert.Equal(AppointmentStatus.Rescheduled, original!.Status);
    }

    [Fact]
    public async Task RescheduleAsync_CarriesOverServiceItemsFromOriginalAppointment()
    {
        var (clientId, serviceId, _) = await SeedBasicDataAsync();
        var originalDate = NextMonday();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, originalDate, new TimeOnly(10, 0), [serviceId], null));

        var result = await _sut.RescheduleAsync(new RescheduleAppointmentRequest(
            created.Value.Id, originalDate.AddDays(1), new TimeOnly(11, 0)));

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Services);
        Assert.Equal("Corte", result.Value.Services[0].Name);
    }

    [Fact]
    public async Task RescheduleAsync_OnAlreadyCompletedAppointment_ReturnsInvalidTransition()
    {
        var (clientId, serviceId, paymentMethodId) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateAppointmentRequest(
            clientId, _professionalId, NextMonday(), new TimeOnly(10, 0), [serviceId], null));
        await _sut.FinishAsync(new FinishAppointmentRequest(created.Value.Id, 10000m, null, null, paymentMethodId, null));

        var result = await _sut.RescheduleAsync(new RescheduleAppointmentRequest(
            created.Value.Id, NextMonday().AddDays(1), new TimeOnly(11, 0)));

        Assert.True(result.IsFailure);
        Assert.Equal("Appointment.InvalidTransition", result.Error.Code);
    }
}
