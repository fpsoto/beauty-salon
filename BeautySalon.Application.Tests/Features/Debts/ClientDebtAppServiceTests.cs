using BeautySalon.Application.Features.Debts;
using BeautySalon.Domain.Entities;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Debts;

public sealed class ClientDebtAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ClientDebtAppService _sut;
    private readonly Guid _professionalId = Guid.NewGuid();

    public ClientDebtAppServiceTests()
    {
        _sut = new ClientDebtAppService(
            _db.UnitOfWork,
            new CreateChargeRequestValidator(),
            new CreatePaymentRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private async Task<(Guid ClientId, Guid PaymentMethodId)> SeedBasicDataAsync()
    {
        var professional = new User { Id = _professionalId, Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" };
        _db.Context.Add(professional);

        var paymentMethod = new PaymentMethod { Name = "Efectivo" };
        _db.UnitOfWork.PaymentMethods.Add(paymentMethod);

        var client = new Client
        {
            Name = "Maria",
            LastName = "Gonzalez",
            Rut = BeautySalon.Domain.ValueObjects.Rut.Create("12345678-5"),
            Phone = "+56911111111"
        };
        _db.UnitOfWork.Clients.Add(client);

        await _db.UnitOfWork.SaveChangesAsync();

        return (client.Id, paymentMethod.Id);
    }

    [Fact]
    public async Task AddChargeAsync_Valid_IncreasesBalance()
    {
        var (clientId, _) = await SeedBasicDataAsync();

        var result = await _sut.AddChargeAsync(new CreateChargeRequest(
            clientId, 5000m, "Corte + manicure", DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsSuccess);

        var history = await _sut.GetHistoryAsync(clientId);
        Assert.Single(history.Value);
        Assert.Equal(5000m, history.Value[0].Amount);
    }

    [Fact]
    public async Task AddPaymentAsync_LessThanBalance_DecreasesBalance()
    {
        var (clientId, paymentMethodId) = await SeedBasicDataAsync();
        await _sut.AddChargeAsync(new CreateChargeRequest(clientId, 5000m, null, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var result = await _sut.AddPaymentAsync(new CreatePaymentRequest(
            clientId, 3000m, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsSuccess);

        var balances = await _sut.GetOutstandingBalancesAsync();
        Assert.Equal(2000m, balances.Value.Single(b => b.ClientId == clientId).Balance);
    }

    [Fact]
    public async Task AddPaymentAsync_ExceedsBalance_ReturnsValidationError()
    {
        var (clientId, paymentMethodId) = await SeedBasicDataAsync();
        await _sut.AddChargeAsync(new CreateChargeRequest(clientId, 5000m, null, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var result = await _sut.AddPaymentAsync(new CreatePaymentRequest(
            clientId, 6000m, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsFailure);
        Assert.Equal("Debt.PaymentExceedsBalance", result.Error.Code);
    }

    [Fact]
    public async Task GetOutstandingBalancesAsync_OnlyReturnsPositiveBalances()
    {
        var (clientId, paymentMethodId) = await SeedBasicDataAsync();
        await _sut.AddChargeAsync(new CreateChargeRequest(clientId, 5000m, null, DateOnly.FromDateTime(DateTime.Today), _professionalId));
        await _sut.AddPaymentAsync(new CreatePaymentRequest(clientId, 5000m, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var balances = await _sut.GetOutstandingBalancesAsync();

        Assert.True(balances.IsSuccess);
        Assert.DoesNotContain(balances.Value, b => b.ClientId == clientId);
    }

    [Fact]
    public async Task DeleteEntryAsync_RemovesFromHistory()
    {
        var (clientId, _) = await SeedBasicDataAsync();
        var created = await _sut.AddChargeAsync(new CreateChargeRequest(clientId, 5000m, null, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var result = await _sut.DeleteEntryAsync(created.Value.Id);

        Assert.True(result.IsSuccess);

        var history = await _sut.GetHistoryAsync(clientId);
        Assert.Empty(history.Value);
    }
}
