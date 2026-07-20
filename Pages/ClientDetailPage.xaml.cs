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
    }

    private async void OnEditClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("client-form", new Dictionary<string, object> { ["ClientId"] = _clientId });
}
