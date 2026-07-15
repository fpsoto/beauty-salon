using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(AppointmentId), "AppointmentId")]
public partial class FinishAppointmentPage : ContentPage
{
    private readonly FinishAppointmentViewModel _viewModel;

    public Guid AppointmentId
    {
        set => _viewModel.Initialize(value);
    }

    public FinishAppointmentPage(FinishAppointmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadPaymentMethodsCommand.ExecuteAsync(null);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FinishAppointmentViewModel.Finished) && _viewModel.Finished)
            await Shell.Current.GoToAsync("..");
    }
}
