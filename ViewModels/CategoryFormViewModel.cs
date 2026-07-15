using BeautySalon.Application.Features.Catalog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class CategoryFormViewModel : ViewModelBase
{
    private readonly ICatalogAppService _catalogAppService;
    private Guid? _categoryId;

    public CategoryFormViewModel(ICatalogAppService catalogAppService, ILogger<CategoryFormViewModel> logger) : base(logger)
    {
        _catalogAppService = catalogAppService;
    }

    [ObservableProperty]
    private string title = "Nueva categoría";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string colorHex = "#512BD4";

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool saved;

    public void Initialize(ServiceCategoryDto? existing)
    {
        if (existing is null)
            return;

        _categoryId = existing.Id;
        Title = "Editar categoría";
        Name = existing.Name;
        ColorHex = existing.ColorHex;
        IsActive = existing.IsActive;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (_categoryId is { } id)
        {
            var result = await _catalogAppService.UpdateCategoryAsync(id, new UpdateServiceCategoryRequest(Name, ColorHex, IsActive));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }
        else
        {
            var result = await _catalogAppService.CreateCategoryAsync(new CreateServiceCategoryRequest(Name, ColorHex));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }

        Saved = true;
    });
}
