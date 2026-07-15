using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Catalog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class CatalogViewModel : ViewModelBase
{
    private readonly ICatalogAppService _catalogAppService;

    public CatalogViewModel(ICatalogAppService catalogAppService, ILogger<CatalogViewModel> logger) : base(logger)
    {
        _catalogAppService = catalogAppService;
    }

    public ObservableCollection<CategoryServiceGroup> Groups { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(LoadCoreAsync);

    [RelayCommand]
    private Task DeleteCategoryAsync(ServiceCategoryDto category) => SafeExecuteAsync(async () =>
    {
        var result = await _catalogAppService.DeleteCategoryAsync(category.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task DeleteServiceAsync(SalonServiceDto service) => SafeExecuteAsync(async () =>
    {
        var result = await _catalogAppService.DeleteServiceAsync(service.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadCoreAsync();
    });

    private async Task LoadCoreAsync()
    {
        var categoriesResult = await _catalogAppService.GetCategoriesAsync(false);
        if (categoriesResult.IsFailure)
        {
            SetError(categoriesResult.Error);
            return;
        }

        var servicesResult = await _catalogAppService.GetServicesAsync(null, false);
        if (servicesResult.IsFailure)
        {
            SetError(servicesResult.Error);
            return;
        }

        Groups.Clear();
        foreach (var category in categoriesResult.Value)
        {
            var services = servicesResult.Value.Where(s => s.CategoryId == category.Id).ToList();
            Groups.Add(new CategoryServiceGroup(category, services));
        }
    }
}
