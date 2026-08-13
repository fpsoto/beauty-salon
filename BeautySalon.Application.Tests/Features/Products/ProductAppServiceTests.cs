using BeautySalon.Application.Features.Products;
using BeautySalon.Domain.Entities;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Products;

public sealed class ProductAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ProductAppService _sut;

    public ProductAppServiceTests()
    {
        _sut = new ProductAppService(
            _db.UnitOfWork,
            new CreateProductRequestValidator(),
            new UpdateProductRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));

        Assert.True(result.IsSuccess);
        Assert.Equal("Shampoo", result.Value.Name);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithZeroPrice_ReturnsValidationError()
    {
        var result = await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 0m));

        Assert.True(result.IsFailure);
        Assert.Equal("Product.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsConflict()
    {
        await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));

        var result = await _sut.CreateAsync(new CreateProductRequest("shampoo", "Otra descripción", 9000m));

        Assert.True(result.IsFailure);
        Assert.Equal("Product.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_RenamingToAnotherExistingName_ReturnsConflict()
    {
        await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));
        var second = await _sut.CreateAsync(new CreateProductRequest("Acondicionador", null, 7000m));

        var result = await _sut.UpdateAsync(second.Value.Id, new UpdateProductRequest("SHAMPOO", null, 7000m, true));

        Assert.True(result.IsFailure);
        Assert.Equal("Product.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_TogglingActive_Persists()
    {
        var created = await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));

        var result = await _sut.UpdateAsync(created.Value.Id, new UpdateProductRequest("Shampoo", null, 8000m, false));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_WithNoHistory_Succeeds()
    {
        var created = await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));

        var result = await _sut.DeleteAsync(created.Value.Id);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_WithSaleHistory_ReturnsConflict()
    {
        var created = await _sut.CreateAsync(new CreateProductRequest("Shampoo", null, 8000m));

        var professional = new User { Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" };
        var paymentMethod = new PaymentMethod { Name = "Efectivo" };
        _db.Context.Add(professional);
        _db.Context.Add(paymentMethod);
        await _db.Context.SaveChangesAsync();

        _db.Context.Add(new ProductSale
        {
            ProductId = created.Value.Id,
            SnapshotProductName = created.Value.Name,
            SnapshotUnitPrice = created.Value.SalePrice,
            Quantity = 1,
            PaymentMethodId = paymentMethod.Id,
            SaleDate = DateOnly.FromDateTime(DateTime.Today),
            ProfessionalId = professional.Id
        });
        await _db.Context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(created.Value.Id);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.HasHistory", result.Error.Code);
    }
}
