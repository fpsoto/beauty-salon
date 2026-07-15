using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Catalog;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(Service), "Service")]
public partial class ServiceFormPage : ContentPage
{
    private readonly ServiceFormViewModel _viewModel;

    public SalonServiceDto? Service
    {
        set => _viewModel.Initialize(value);
    }

    public ServiceFormPage(ServiceFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServiceFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
