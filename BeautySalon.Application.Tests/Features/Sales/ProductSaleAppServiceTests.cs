using BeautySalon.Application.Features.Sales;
using BeautySalon.Domain.Entities;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Sales;

public sealed class ProductSaleAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ProductSaleAppService _sut;
    private readonly Guid _professionalId = Guid.NewGuid();

    public ProductSaleAppServiceTests()
    {
        _sut = new ProductSaleAppService(
            _db.UnitOfWork,
            new CreateProductSaleRequestValidator(),
            new UpdateProductSaleRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private async Task<(Guid ProductId, Guid PaymentMethodId, Guid ClientId)> SeedBasicDataAsync()
    {
        var professional = new User { Id = _professionalId, Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" };
        _db.Context.Add(professional);

        var product = new Product { Name = "Shampoo", SalePrice = 8000m, IsActive = true };
        _db.UnitOfWork.Products.Add(product);

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

        return (product.Id, paymentMethod.Id, client.Id);
    }

    [Fact]
    public async Task CreateAsync_WithNoClient_SucceedsAsWalkInSale()
    {
        var (productId, paymentMethodId, _) = await SeedBasicDataAsync();

        var result = await _sut.CreateAsync(new CreateProductSaleRequest(
            productId, 2, 8000m, null, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ClientId);
        Assert.Equal(16000m, result.Value.Total);
    }

    [Fact]
    public async Task CreateAsync_WithClient_Succeeds()
    {
        var (productId, paymentMethodId, clientId) = await SeedBasicDataAsync();

        var result = await _sut.CreateAsync(new CreateProductSaleRequest(
            productId, 1, 8000m, clientId, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsSuccess);
        Assert.Equal(clientId, result.Value.ClientId);
        Assert.Equal("Maria Gonzalez", result.Value.ClientFullName);
    }

    [Fact]
    public async Task CreateAsync_WithInactiveProduct_ReturnsValidationError()
    {
        var (productId, paymentMethodId, _) = await SeedBasicDataAsync();
        var product = await _db.UnitOfWork.Products.GetByIdAsync(productId);
        product!.IsActive = false;
        await _db.UnitOfWork.SaveChangesAsync();

        var result = await _sut.CreateAsync(new CreateProductSaleRequest(
            productId, 1, 8000m, null, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        Assert.True(result.IsFailure);
        Assert.Equal("Product.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_ChangesQuantityAndPrice_Persists()
    {
        var (productId, paymentMethodId, _) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateProductSaleRequest(
            productId, 1, 8000m, null, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var result = await _sut.UpdateAsync(created.Value.Id, new UpdateProductSaleRequest(
            3, 7500m, null, paymentMethodId, DateOnly.FromDateTime(DateTime.Today)));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Quantity);
        Assert.Equal(22500m, result.Value.Total);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSale()
    {
        var (productId, paymentMethodId, _) = await SeedBasicDataAsync();
        var created = await _sut.CreateAsync(new CreateProductSaleRequest(
            productId, 1, 8000m, null, paymentMethodId, DateOnly.FromDateTime(DateTime.Today), _professionalId));

        var result = await _sut.DeleteAsync(created.Value.Id);

        Assert.True(result.IsSuccess);

        var sales = await _sut.GetByDateRangeAsync(
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today), _professionalId);
        Assert.Empty(sales.Value);
    }
}
