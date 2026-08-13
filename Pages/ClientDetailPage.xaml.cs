using Beauty_Salon.Resources.Strings;
using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(ClientId), "ClientId")]
public partial class ClientDetailPage : ContentPage
{
    private readonly ClientDetailViewModel _viewModel;
    private Guid _clientId;

    public Guid ClientId
    {
        set
        {
            _clientId = value;
            _ = _viewModel.LoadAsync(value);
        }
    }

    public ClientDetailPage(ClientDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_clientId != Guid.Empty)
            _ = _viewModel.LoadAsync(_clientId);
    }

    private async void OnEditClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("client-form", new Dictionary<string, object> { ["ClientId"] = _clientId });

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Detail is null)
            return;

        var clientName = $"{_viewModel.Detail.Client.Name} {_viewModel.Detail.Client.LastName}";
        var confirmed = await DisplayAlertAsync(clientName, AppResources.DeleteConfirmMessage, AppResources.Delete, AppResources.Close);
        if (confirmed)
            await _viewModel.DeleteCommand.ExecuteAsync(null);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClientDetailViewModel.Deleted) && _viewModel.Deleted)
            await Shell.Current.GoToAsync("..");
    }
}
