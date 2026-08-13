using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Sales;

public sealed class ProductSaleAppService : IProductSaleAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductSaleRequest> _createValidator;
    private readonly IValidator<UpdateProductSaleRequest> _updateValidator;

    public ProductSaleAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreateProductSaleRequest> createValidator,
        IValidator<UpdateProductSaleRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IReadOnlyList<ProductSaleDto>>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.ProductSales.GetByDateRangeAsync(from, to, professionalId, cancellationToken);
        return Result.Success<IReadOnlyList<ProductSaleDto>>(sales.Select(s => s.ToDto()).ToList());
    }

    public async Task<Result<ProductSaleDto>> CreateAsync(CreateProductSaleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ProductSaleDto>(Error.Validation("ProductSale.Invalid", validation.ToString(" ")));

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductSaleDto>(Error.NotFound("Product.NotFound", "Producto no encontrado."));
        if (!product.IsActive)
            return Result.Failure<ProductSaleDto>(Error.Validation("Product.Inactive", "El producto está desactivado."));

        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null)
            return Result.Failure<ProductSaleDto>(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));
        if (!paymentMethod.IsActive)
            return Result.Failure<ProductSaleDto>(Error.Validation("PaymentMethod.Inactive", "El método de pago está desactivado."));

        Client? client = null;
        if (request.ClientId is { } clientId)
        {
            client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
            if (client is null)
                return Result.Failure<ProductSaleDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));
        }

        var sale = new ProductSale
        {
            ProductId = product.Id,
            SnapshotProductName = product.Name,
            SnapshotUnitPrice = request.UnitPrice,
            Quantity = request.Quantity,
            ClientId = request.ClientId,
            PaymentMethodId = request.PaymentMethodId,
            SaleDate = request.SaleDate,
            ProfessionalId = request.ProfessionalId
        };

        _unitOfWork.ProductSales.Add(sale);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        sale.Client = client;
        sale.PaymentMethod = paymentMethod;
        return Result.Success(sale.ToDto());
    }

    public async Task<Result<ProductSaleDto>> UpdateAsync(Guid saleId, UpdateProductSaleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ProductSaleDto>(Error.Validation("ProductSale.Invalid", validation.ToString(" ")));

        var sale = await _unitOfWork.ProductSales.GetByIdAsync(saleId, cancellationToken);
        if (sale is null)
            return Result.Failure<ProductSaleDto>(Error.NotFound("ProductSale.NotFound", "Venta no encontrada."));

        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null)
            return Result.Failure<ProductSaleDto>(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));
        if (!paymentMethod.IsActive)
            return Result.Failure<ProductSaleDto>(Error.Validation("PaymentMethod.Inactive", "El método de pago está desactivado."));

        Client? client = null;
        if (request.ClientId is { } clientId)
        {
            client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
            if (client is null)
                return Result.Failure<ProductSaleDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));
        }

        sale.Quantity = request.Quantity;
        sale.SnapshotUnitPrice = request.UnitPrice;
        sale.ClientId = request.ClientId;
        sale.PaymentMethodId = request.PaymentMethodId;
        sale.SaleDate = request.SaleDate;

        _unitOfWork.ProductSales.Update(sale);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        sale.Client = client;
        sale.PaymentMethod = paymentMethod;
        return Result.Success(sale.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.ProductSales.GetByIdAsync(saleId, cancellationToken);
        if (sale is null)
            return Result.Failure(Error.NotFound("ProductSale.NotFound", "Venta no encontrada."));

        _unitOfWork.ProductSales.Remove(sale);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
