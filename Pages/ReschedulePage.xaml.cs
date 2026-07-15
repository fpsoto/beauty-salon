using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(AppointmentId), "AppointmentId")]
public partial class ReschedulePage : ContentPage
{
    private readonly RescheduleViewModel _viewModel;

    public Guid AppointmentId
    {
        set => _viewModel.Initialize(value);
    }

    public ReschedulePage(RescheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RescheduleViewModel.Rescheduled) && _viewModel.Rescheduled)
            await Shell.Current.GoToAsync("..");
    }
}
