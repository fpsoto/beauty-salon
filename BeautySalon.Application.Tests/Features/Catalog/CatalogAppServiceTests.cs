using BeautySalon.Application.Features.Catalog;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Catalog;

public sealed class CatalogAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly CatalogAppService _sut;

    public CatalogAppServiceTests()
    {
        _sut = new CatalogAppService(
            _db.UnitOfWork,
            new CreateServiceCategoryRequestValidator(),
            new UpdateServiceCategoryRequestValidator(),
            new CreateSalonServiceRequestValidator(),
            new UpdateSalonServiceRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private async Task<Guid> CreateCategoryAsync(string name = "Cabello")
    {
        var result = await _sut.CreateCategoryAsync(new CreateServiceCategoryRequest(name, "#2563EB"));
        return result.Value.Id;
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateCategoryAsync(new CreateServiceCategoryRequest("Cabello", "#2563EB"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Cabello", result.Value.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithInvalidColorHex_ReturnsValidationError()
    {
        var result = await _sut.CreateCategoryAsync(new CreateServiceCategoryRequest("Cabello", "not-a-color"));

        Assert.True(result.IsFailure);
        Assert.Equal("Category.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ReturnsConflict()
    {
        await CreateCategoryAsync("Cabello");

        var result = await _sut.CreateCategoryAsync(new CreateServiceCategoryRequest("cabello", "#2563EB"));

        Assert.True(result.IsFailure);
        Assert.Equal("Category.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task UpdateCategoryAsync_RenamingToAnotherExistingName_ReturnsConflict()
    {
        await CreateCategoryAsync("Cabello");
        var otherCategoryId = await CreateCategoryAsync("Masajes");

        var result = await _sut.UpdateCategoryAsync(otherCategoryId, new UpdateServiceCategoryRequest("CABELLO", "#2563EB", true));

        Assert.True(result.IsFailure);
        Assert.Equal("Category.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task CreateServiceAsync_WithDuplicateNameInSameCategory_ReturnsConflict()
    {
        var categoryId = await CreateCategoryAsync();
        await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));

        var result = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("corte", categoryId, 12000m, 45, null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("Service.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task CreateServiceAsync_WithSameNameInDifferentCategory_Succeeds()
    {
        var categoryId = await CreateCategoryAsync("Cabello");
        var otherCategoryId = await CreateCategoryAsync("Masajes");
        await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));

        var result = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", otherCategoryId, 12000m, 45, null, null));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateServiceAsync_RenamingToAnotherExistingNameInSameCategory_ReturnsConflict()
    {
        var categoryId = await CreateCategoryAsync();
        await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));
        var second = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Peinado", categoryId, 8000m, 20, null, null));

        var result = await _sut.UpdateServiceAsync(second.Value.Id, new UpdateSalonServiceRequest("CORTE", categoryId, 8000m, 20, null, null, true));

        Assert.True(result.IsFailure);
        Assert.Equal("Service.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithNoServices_Succeeds()
    {
        var categoryId = await CreateCategoryAsync();

        var result = await _sut.DeleteCategoryAsync(categoryId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithServices_ReturnsConflict()
    {
        var categoryId = await CreateCategoryAsync();
        await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));

        var result = await _sut.DeleteCategoryAsync(categoryId);

        Assert.True(result.IsFailure);
        Assert.Equal("Category.HasServices", result.Error.Code);
    }

    [Fact]
    public async Task CreateServiceAsync_WithUnknownCategory_ReturnsNotFound()
    {
        var result = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", Guid.NewGuid(), 10000m, 30, null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("Category.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task DeleteServiceAsync_WithNoHistory_Succeeds()
    {
        var categoryId = await CreateCategoryAsync();
        var service = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));

        var result = await _sut.DeleteServiceAsync(service.Value.Id);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteServiceAsync_WithAppointmentHistory_ReturnsConflict()
    {
        var categoryId = await CreateCategoryAsync();
        var service = await _sut.CreateServiceAsync(new CreateSalonServiceRequest("Corte", categoryId, 10000m, 30, null, null));

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
            ServiceItems =
            [
                new AppointmentServiceItem { ServiceId = service.Value.Id, SnapshotServiceName = "Corte", SnapshotPrice = 10000m, SnapshotDurationMinutes = 30 }
            ]
        });
        await _db.Context.SaveChangesAsync();

        var result = await _sut.DeleteServiceAsync(service.Value.Id);

        Assert.True(result.IsFailure);
        Assert.Equal("Service.HasHistory", result.Error.Code);
    }
}
