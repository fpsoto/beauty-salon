using System.Collections.ObjectModel;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Sales;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IProductSaleAppService _productSaleAppService;
    private readonly ICurrentUserContext _currentUserContext;

    // Falls back to the seeded admin only as a defensive default - this page is only
    // reachable post-login, so _currentUserContext.UserId should always be set by then.
    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public SalesViewModel(IProductSaleAppService productSaleAppService, ICurrentUserContext currentUserContext, ILogger<SalesViewModel> logger) : base(logger)
    {
        _productSaleAppService = productSaleAppService;
        _currentUserContext = currentUserContext;

        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = FromDate.AddMonths(1).AddDays(-1);
    }

    [ObservableProperty]
    private DateTime fromDate;

    [ObservableProperty]
    private DateTime toDate;

    public ObservableCollection<ProductSaleDto> Sales { get; } = [];

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
    private Task DeleteAsync(ProductSaleDto sale) => SafeExecuteAsync(async () =>
    {
        var result = await _productSaleAppService.DeleteAsync(sale.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadCoreAsync();
    });

    private async Task LoadCoreAsync()
    {
        var result = await _productSaleAppService.GetByDateRangeAsync(
            DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), ProfessionalId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Sales.Clear();
        foreach (var sale in result.Value)
            Sales.Add(sale);
    }
}
