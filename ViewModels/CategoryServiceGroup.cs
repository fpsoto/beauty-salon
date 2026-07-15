using BeautySalon.Application.Features.Catalog;

namespace Beauty_Salon.ViewModels;

public sealed class CategoryServiceGroup : List<SalonServiceDto>
{
    public CategoryServiceGroup(ServiceCategoryDto category, IEnumerable<SalonServiceDto> services) : base(services)
    {
        Category = category;
    }

    public ServiceCategoryDto Category { get; }
}
