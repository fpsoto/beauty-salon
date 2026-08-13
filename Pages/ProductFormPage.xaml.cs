using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Products;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(Product), "Product")]
public partial class ProductFormPage : ContentPage
{
    private readonly ProductFormViewModel _viewModel;

    public ProductDto? Product
    {
        set => _viewModel.Initialize(value);
    }

    public ProductFormPage(ProductFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
