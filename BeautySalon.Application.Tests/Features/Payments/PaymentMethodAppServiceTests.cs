using BeautySalon.Application.Features.Payments;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Payments;

public sealed class PaymentMethodAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly PaymentMethodAppService _sut;

    public PaymentMethodAppServiceTests()
    {
        _sut = new PaymentMethodAppService(
            _db.UnitOfWork,
            new CreatePaymentMethodRequestValidator(),
            new UpdatePaymentMethodRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        Assert.True(result.IsSuccess);
        Assert.Equal("Efectivo", result.Value.Name);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsValidationError()
    {
        var result = await _sut.CreateAsync(new CreatePaymentMethodRequest("", 0));

        Assert.True(result.IsFailure);
        Assert.Equal("PaymentMethod.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsConflict()
    {
        await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        var result = await _sut.CreateAsync(new CreatePaymentMethodRequest("efectivo", 1));

        Assert.True(result.IsFailure);
        Assert.Equal("PaymentMethod.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_RenamingToAnotherExistingName_ReturnsConflict()
    {
        await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));
        var second = await _sut.CreateAsync(new CreatePaymentMethodRequest("Débito", 1));

        var result = await _sut.UpdateAsync(second.Value.Id, new UpdatePaymentMethodRequest("EFECTIVO", 1, true));

        Assert.True(result.IsFailure);
        Assert.Equal("PaymentMethod.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_KeepingSameName_Succeeds()
    {
        var created = await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        var result = await _sut.UpdateAsync(created.Value.Id, new UpdatePaymentMethodRequest("Efectivo", 5, true));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_TogglingActive_Persists()
    {
        var created = await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        var result = await _sut.UpdateAsync(created.Value.Id, new UpdatePaymentMethodRequest("Efectivo", 0, false));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_WithNoHistory_Succeeds()
    {
        var created = await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        var result = await _sut.DeleteAsync(created.Value.Id);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_WithAppointmentHistory_ReturnsConflict()
    {
        var created = await _sut.CreateAsync(new CreatePaymentMethodRequest("Efectivo", 0));

        var professional = new User { Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" };
        var client = new Client { Name = "Maria", LastName = "Gonzalez", Rut = BeautySalon.Domain.ValueObjects.Rut.Create("12345678-5"), Phone = "+56911111111" };
        _db.Context.Add(professional);
        _db.Context.Add(client);
        await _db.Context.SaveChangesAsync();

        _db.Context.Add(new Appointment
        {
            ClientId = client.Id,
            ProfessionalId = professional.Id,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            Status = AppointmentStatus.Completed,
            SuggestedPrice = 10000m,
            PaymentMethodId = created.Value.Id
        });
        await _db.Context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(created.Value.Id);

        Assert.True(result.IsFailure);
        Assert.Equal("PaymentMethod.HasHistory", result.Error.Code);
    }
}
