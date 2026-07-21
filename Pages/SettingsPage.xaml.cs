using Beauty_Salon.Resources.Strings;
using Beauty_Salon.Services;
using Beauty_Salon.ViewModels;
using BeautySalon.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Beauty_Salon.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IDataBackupService _dataBackupService;
    private readonly ISessionService _sessionService;
    private readonly IPersistedSessionStore _persistedSessionStore;
    private readonly IServiceProvider _serviceProvider;

    public SettingsPage(
        SettingsViewModel viewModel,
        IDataBackupService dataBackupService,
        ISessionService sessionService,
        IPersistedSessionStore persistedSessionStore,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _dataBackupService = dataBackupService;
        _sessionService = sessionService;
        _persistedSessionStore = persistedSessionStore;
        _serviceProvider = serviceProvider;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnHelpClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("help");

    private async void OnBackupClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await _dataBackupService.ExportBackupAsync();
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = AppResources.BackupShareTitle,
                File = new ShareFile(path)
            });
        }
        catch
        {
            await DisplayAlertAsync(AppResources.BackupTitle, AppResources.BackupErrorMessage, AppResources.Close);
        }
    }

    private async void OnRestoreClicked(object? sender, EventArgs e)
    {
        FileResult? pickedFile;
        try
        {
            pickedFile = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = AppResources.RestorePickerTitle });
        }
        catch
        {
            return;
        }

        if (pickedFile is null)
            return;

        var confirmed = await DisplayAlertAsync(
            AppResources.RestoreTitle, AppResources.RestoreConfirmMessage, AppResources.RestoreConfirmButton, AppResources.Close);
        if (!confirmed)
            return;

        var result = await _dataBackupService.RestoreAsync(pickedFile.FullPath);
        if (result.IsFailure)
        {
            await DisplayAlertAsync(AppResources.RestoreTitle, result.Error.Message, AppResources.Close);
            return;
        }

        await DisplayAlertAsync(AppResources.RestoreTitle, AppResources.RestoreSuccessMessage, AppResources.Close);
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(AppResources.LogoutButton, AppResources.LogoutConfirmMessage, AppResources.LogoutButton, AppResources.Close);
        if (!confirmed)
            return;

        _sessionService.SignOut();
        _persistedSessionStore.Clear();
        var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        Application.Current!.Windows[0].Page = loginPage;
    }
}
