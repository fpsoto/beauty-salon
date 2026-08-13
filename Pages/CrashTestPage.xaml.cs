using Plugin.Firebase.Crashlytics;

namespace Beauty_Salon.Pages;

public partial class CrashTestPage : ContentPage
{
    public CrashTestPage()
    {
        InitializeComponent();
    }

    private async void OnDivideByZeroClicked(object? sender, EventArgs e)
    {
        try
        {
            var zero = 0;
            _ = 1 / zero;
        }
        catch (Exception ex)
        {
            await ReportAsync(ex);
        }
    }

    private async void OnNullReferenceClicked(object? sender, EventArgs e)
    {
        try
        {
            string? text = null;
            _ = text!.Length;
        }
        catch (Exception ex)
        {
            await ReportAsync(ex);
        }
    }

    private async void OnIndexOutOfRangeClicked(object? sender, EventArgs e)
    {
        try
        {
            var items = new int[3];
            var index = 10;
            _ = items[index];
        }
        catch (Exception ex)
        {
            await ReportAsync(ex);
        }
    }

    private async void OnGenericExceptionClicked(object? sender, EventArgs e)
    {
        try
        {
            throw new InvalidOperationException("Excepción de prueba forzada desde CrashTestPage.");
        }
        catch (Exception ex)
        {
            await ReportAsync(ex);
        }
    }

    private async void OnBackgroundThreadExceptionClicked(object? sender, EventArgs e)
    {
        Exception? caught = null;
        await Task.Run(() =>
        {
            try
            {
                throw new InvalidOperationException("Excepción de prueba en un hilo en segundo plano.");
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });

        if (caught is not null)
            await ReportAsync(caught);
    }

    private async Task ReportAsync(Exception exception)
    {
        CrossFirebaseCrashlytics.Current.RecordException(exception);
        await DisplayAlertAsync("Crashlytics", $"Registrado como no fatal: {exception.GetType().Name}", "OK");
    }
}
