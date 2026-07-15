using BeautySalon.Application.Features.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthAppService _authAppService;

    public LoginViewModel(IAuthAppService authAppService, ILogger<LoginViewModel> logger) : base(logger)
    {
        _authAppService = authAppService;
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool loginSucceeded;

    [RelayCommand]
    private Task LoginAsync() => SafeExecuteAsync(async () =>
    {
        var result = await _authAppService.LoginAsync(new LoginRequest(Username, Password));
        if (result.IsFailure)
        {
            SetError(result.Error);
            LoginSucceeded = false;
            return;
        }

        LoginSucceeded = true;
    });
}
