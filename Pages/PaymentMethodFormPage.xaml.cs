using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Payments;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(PaymentMethod), "PaymentMethod")]
public partial class PaymentMethodFormPage : ContentPage
{
    private readonly PaymentMethodFormViewModel _viewModel;

    public PaymentMethodDto? PaymentMethod
    {
        set => _viewModel.Initialize(value);
    }

    public PaymentMethodFormPage(PaymentMethodFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PaymentMethodFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
