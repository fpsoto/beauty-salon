using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Catalog;

public interface ICatalogAppService
{
    Task<Result<IReadOnlyList<ServiceCategoryDto>>> GetCategoriesAsync(bool onlyActive, CancellationToken cancellationToken = default);
    Task<Result<ServiceCategoryDto>> CreateCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceCategoryDto>> UpdateCategoryAsync(Guid categoryId, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SalonServiceDto>>> GetServicesAsync(Guid? categoryId, bool onlyActive, CancellationToken cancellationToken = default);
    Task<Result<SalonServiceDto>> CreateServiceAsync(CreateSalonServiceRequest request, CancellationToken cancellationToken = default);
    Task<Result<SalonServiceDto>> UpdateServiceAsync(Guid serviceId, UpdateSalonServiceRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
