using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Products;

public sealed class ProductAppService : IProductAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default)
    {
        var products = onlyActive
            ? await _unitOfWork.Products.GetActiveAsync(cancellationToken)
            : await _unitOfWork.Products.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductDto>>(products.Select(p => p.ToDto()).ToList());
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ProductDto>(Error.Validation("Product.Invalid", validation.ToString(" ")));

        var existing = await _unitOfWork.Products.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            return Result.Failure<ProductDto>(Error.Conflict("Product.DuplicateName", "Ya existe un producto con ese nombre."));

        var product = new Product { Name = request.Name, Description = request.Description, SalePrice = request.SalePrice, IsActive = true };
        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToDto());
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ProductDto>(Error.Validation("Product.Invalid", validation.ToString(" ")));

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product.NotFound", "Producto no encontrado."));

        if (!string.Equals(product.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _unitOfWork.Products.GetByNameAsync(request.Name, cancellationToken);
            if (existing is not null && existing.Id != productId)
                return Result.Failure<ProductDto>(Error.Conflict("Product.DuplicateName", "Ya existe un producto con ese nombre."));
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.SalePrice = request.SalePrice;
        product.IsActive = request.IsActive;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("Product.NotFound", "Producto no encontrado."));

        if (await _unitOfWork.Products.HasSaleHistoryAsync(productId, cancellationToken))
            return Result.Failure(Error.Conflict("Product.HasHistory", "No se puede eliminar: el producto tiene historial de ventas. Desactívelo en su lugar."));

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
