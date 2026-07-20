using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthAppService _authAppService;
    private readonly ISessionService _sessionService;

    public LoginViewModel(IAuthAppService authAppService, ISessionService sessionService, ILogger<LoginViewModel> logger) : base(logger)
    {
        _authAppService = authAppService;
        _sessionService = sessionService;
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

        _sessionService.SignIn(result.Value.UserId, result.Value.Username);
        LoginSucceeded = true;
    });
}
