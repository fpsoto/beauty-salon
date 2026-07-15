using System.Collections.ObjectModel;
using Beauty_Salon.Resources.Strings;
using BeautySalon.Application.Features.Catalog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ServiceFormViewModel : ViewModelBase
{
    private readonly ICatalogAppService _catalogAppService;
    private Guid? _serviceId;
    private Guid? _existingCategoryId;

    public ServiceFormViewModel(ICatalogAppService catalogAppService, ILogger<ServiceFormViewModel> logger) : base(logger)
    {
        _catalogAppService = catalogAppService;
    }

    [ObservableProperty]
    private string title = "Nuevo servicio";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ServiceCategoryDto? selectedCategory;

    [ObservableProperty]
    private decimal suggestedPrice;

    [ObservableProperty]
    private int durationMinutes = 30;

    [ObservableProperty]
    private string? colorHex;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool saved;

    public ObservableCollection<ServiceCategoryDto> Categories { get; } = [];

    public void Initialize(SalonServiceDto? existing)
    {
        if (existing is null)
            return;

        _serviceId = existing.Id;
        _existingCategoryId = existing.CategoryId;
        Title = "Editar servicio";
        Name = existing.Name;
        SuggestedPrice = existing.SuggestedPrice;
        DurationMinutes = existing.DurationMinutes;
        ColorHex = existing.ColorHex;
        Description = existing.Description;
        IsActive = existing.IsActive;
    }

    [RelayCommand]
    private Task LoadCategoriesAsync() => SafeExecuteAsync(async () =>
    {
        var result = await _catalogAppService.GetCategoriesAsync(true);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Categories.Clear();
        foreach (var category in result.Value)
            Categories.Add(category);

        if (_existingCategoryId is { } categoryId)
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == categoryId);
    });

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (SelectedCategory is null)
        {
            ErrorMessage = AppResources.SelectCategoryRequired;
            return;
        }

        if (_serviceId is { } id)
        {
            var request = new UpdateSalonServiceRequest(Name, SelectedCategory.Id, SuggestedPrice, DurationMinutes, ColorHex, Description, IsActive);
            var result = await _catalogAppService.UpdateServiceAsync(id, request);
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }
        else
        {
            var request = new CreateSalonServiceRequest(Name, SelectedCategory.Id, SuggestedPrice, DurationMinutes, ColorHex, Description);
            var result = await _catalogAppService.CreateServiceAsync(request);
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }

        Saved = true;
    });
}
