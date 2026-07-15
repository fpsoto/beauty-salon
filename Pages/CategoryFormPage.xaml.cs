using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Catalog;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(Category), "Category")]
public partial class CategoryFormPage : ContentPage
{
    private readonly CategoryFormViewModel _viewModel;

    public ServiceCategoryDto? Category
    {
        set => _viewModel.Initialize(value);
    }

    public CategoryFormPage(CategoryFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CategoryFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
