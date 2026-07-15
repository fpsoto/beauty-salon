using Beauty_Salon.Resources.Strings;
using BeautySalon.Domain.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    private readonly ILogger _logger;

    protected ViewModelBase(ILogger logger)
    {
        _logger = logger;
    }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    // Wraps a command body so an unexpected exception is caught once, logged, and
    // surfaced as ErrorMessage instead of crashing the app. Expected business
    // failures should already come back as a failed Result (see SetError), not an
    // exception - this is the safety net for genuinely unexpected failures only.
    // Not reentrant: don't call another SafeExecuteAsync-wrapped method from inside
    // one - it would see IsBusy already true and no-op. Call the callee's private
    // "Core" method instead.
    protected async Task SafeExecuteAsync(Func<Task> operation)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = AppResources.UnexpectedErrorMessage;
            _logger.LogError(ex, "Unhandled error in {ViewModel}", GetType().Name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void SetError(Error error)
    {
        ErrorMessage = error.Message;
    }
}
