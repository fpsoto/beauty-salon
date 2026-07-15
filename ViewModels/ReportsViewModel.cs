using BeautySalon.Application.Common;
using BeautySalon.Application.Features.Reports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportAppService _reportAppService;

    // TODO(Fase 5+): replace with the signed-in professional's id once login/session wiring supports it.
    private readonly Guid _professionalId = WellKnownIds.AdminUserId;

    public ReportsViewModel(IReportAppService reportAppService, ILogger<ReportsViewModel> logger) : base(logger)
    {
        _reportAppService = reportAppService;
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
    }

    [ObservableProperty]
    private DateTime fromDate;

    [ObservableProperty]
    private DateTime toDate;

    [ObservableProperty]
    private ReportSummaryDto? summary;

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(LoadCoreAsync);

    [RelayCommand]
    private Task SetTodayAsync() => SafeExecuteAsync(async () =>
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task SetThisWeekAsync() => SafeExecuteAsync(async () =>
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = today.AddDays(-diff);
        FromDate = monday.ToDateTime(TimeOnly.MinValue);
        ToDate = monday.AddDays(6).ToDateTime(TimeOnly.MinValue);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task SetThisMonthAsync() => SafeExecuteAsync(async () =>
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = FromDate.AddMonths(1).AddDays(-1);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task SetThisYearAsync() => SafeExecuteAsync(async () =>
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, 1, 1);
        ToDate = new DateTime(today.Year, 12, 31);
        await LoadCoreAsync();
    });

    private async Task LoadCoreAsync()
    {
        var result = await _reportAppService.GetSummaryAsync(
            DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), _professionalId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Summary = result.Value;
    }
}
