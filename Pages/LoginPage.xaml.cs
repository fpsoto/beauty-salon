using Beauty_Salon.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Beauty_Salon.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public LoginPage(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        BindingContext = viewModel;
    }

    private async void OnPasswordCompleted(object? sender, EventArgs e) => await LoginAsync();

    private async void OnLoginClicked(object? sender, EventArgs e) => await LoginAsync();

    private async Task LoginAsync()
    {
        await _viewModel.LoginCommand.ExecuteAsync(null);

        if (_viewModel.LoginSucceeded)
        {
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            Application.Current!.Windows[0].Page = shell;
        }
    }
}
