using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Catalog;

public sealed class CatalogAppService : ICatalogAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateServiceCategoryRequest> _createCategoryValidator;
    private readonly IValidator<UpdateServiceCategoryRequest> _updateCategoryValidator;
    private readonly IValidator<CreateSalonServiceRequest> _createServiceValidator;
    private readonly IValidator<UpdateSalonServiceRequest> _updateServiceValidator;

    public CatalogAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreateServiceCategoryRequest> createCategoryValidator,
        IValidator<UpdateServiceCategoryRequest> updateCategoryValidator,
        IValidator<CreateSalonServiceRequest> createServiceValidator,
        IValidator<UpdateSalonServiceRequest> updateServiceValidator)
    {
        _unitOfWork = unitOfWork;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
        _createServiceValidator = createServiceValidator;
        _updateServiceValidator = updateServiceValidator;
    }

    public async Task<Result<IReadOnlyList<ServiceCategoryDto>>> GetCategoriesAsync(bool onlyActive, CancellationToken cancellationToken = default)
    {
        var categories = onlyActive
            ? await _unitOfWork.ServiceCategories.GetActiveAsync(cancellationToken)
            : await _unitOfWork.ServiceCategories.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ServiceCategoryDto>>(categories.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<ServiceCategoryDto>> CreateCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createCategoryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ServiceCategoryDto>(Error.Validation("Category.Invalid", validation.ToString(" ")));

        var category = new ServiceCategory { Name = request.Name, ColorHex = request.ColorHex, IsActive = true };
        _unitOfWork.ServiceCategories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.ToDto());
    }

    public async Task<Result<ServiceCategoryDto>> UpdateCategoryAsync(Guid categoryId, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateCategoryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ServiceCategoryDto>(Error.Validation("Category.Invalid", validation.ToString(" ")));

        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
            return Result.Failure<ServiceCategoryDto>(Error.NotFound("Category.NotFound", "Categoría no encontrada."));

        category.Name = request.Name;
        category.ColorHex = request.ColorHex;
        category.IsActive = request.IsActive;

        _unitOfWork.ServiceCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.ToDto());
    }

    public async Task<Result> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Categoría no encontrada."));

        var services = await _unitOfWork.SalonServices.GetByCategoryAsync(categoryId, cancellationToken);
        if (services.Count > 0)
            return Result.Failure(Error.Conflict("Category.HasServices", "No se puede eliminar: la categoría tiene services asociados. Desactívela en su lugar."));

        _unitOfWork.ServiceCategories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<SalonServiceDto>>> GetServicesAsync(Guid? categoryId, bool onlyActive, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SalonService> services = categoryId is not null
            ? await _unitOfWork.SalonServices.GetByCategoryAsync(categoryId.Value, cancellationToken)
            : onlyActive
                ? await _unitOfWork.SalonServices.GetActiveAsync(cancellationToken)
                : await _unitOfWork.SalonServices.GetAllAsync(cancellationToken);

        if (onlyActive && categoryId is not null)
            services = services.Where(s => s.IsActive).ToList();

        return Result.Success<IReadOnlyList<SalonServiceDto>>(services.Select(s => s.ToDto()).ToList());
    }

    public async Task<Result<SalonServiceDto>> CreateServiceAsync(CreateSalonServiceRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createServiceValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<SalonServiceDto>(Error.Validation("Service.Invalid", validation.ToString(" ")));

        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<SalonServiceDto>(Error.NotFound("Category.NotFound", "Categoría no encontrada."));

        var service = new SalonService
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            SuggestedPrice = request.SuggestedPrice,
            DurationMinutes = request.DurationMinutes,
            ColorHex = request.ColorHex,
            Description = request.Description,
            IsActive = true
        };

        _unitOfWork.SalonServices.Add(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.ToDto(category.Name));
    }

    public async Task<Result<SalonServiceDto>> UpdateServiceAsync(Guid serviceId, UpdateSalonServiceRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateServiceValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<SalonServiceDto>(Error.Validation("Service.Invalid", validation.ToString(" ")));

        var service = await _unitOfWork.SalonServices.GetByIdAsync(serviceId, cancellationToken);
        if (service is null)
            return Result.Failure<SalonServiceDto>(Error.NotFound("Service.NotFound", "Servicio no encontrado."));

        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<SalonServiceDto>(Error.NotFound("Category.NotFound", "Categoría no encontrada."));

        service.Name = request.Name;
        service.CategoryId = request.CategoryId;
        service.SuggestedPrice = request.SuggestedPrice;
        service.DurationMinutes = request.DurationMinutes;
        service.ColorHex = request.ColorHex;
        service.Description = request.Description;
        service.IsActive = request.IsActive;

        _unitOfWork.SalonServices.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.ToDto(category.Name));
    }

    public async Task<Result> DeleteServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var service = await _unitOfWork.SalonServices.GetByIdAsync(serviceId, cancellationToken);
        if (service is null)
            return Result.Failure(Error.NotFound("Service.NotFound", "Servicio no encontrado."));

        if (await _unitOfWork.SalonServices.HasAppointmentHistoryAsync(serviceId, cancellationToken))
            return Result.Failure(Error.Conflict("Service.HasHistory", "No se puede eliminar: el servicio tiene historial de citas. Desactívelo en su lugar."));

        _unitOfWork.SalonServices.Remove(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
