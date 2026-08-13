using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Debts;
using BeautySalon.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly IClientAppService _clientAppService;
    private readonly IClientDebtAppService _clientDebtAppService;

    public ClientDetailViewModel(
        IClientAppService clientAppService,
        IClientDebtAppService clientDebtAppService,
        ILogger<ClientDetailViewModel> logger) : base(logger)
    {
        _clientAppService = clientAppService;
        _clientDebtAppService = clientDebtAppService;
    }

    [NotifyPropertyChangedFor(nameof(Initials))]
    [ObservableProperty]
    private ClientDetailDto? detail;

    [ObservableProperty]
    private bool deleted;

    // Never stored - always Sum(Charge) - Sum(Payment) over DebtEntries, same
    // "compute don't persist" convention as ProductSale.Total.
    [NotifyPropertyChangedFor(nameof(HasOutstandingBalance))]
    [ObservableProperty]
    private decimal balance;

    public ObservableCollection<ClientDebtEntryDto> DebtEntries { get; } = [];

    public bool HasOutstandingBalance => Balance > 0;

    // DebtEntries never fires its own PropertyChanged (only Balance does, via
    // NotifyPropertyChangedFor above) - manually notified in LoadAsync right after
    // the collection is repopulated, same reasoning CountToBoolConverter's own doc
    // comment gives for why plain collection bindings don't refresh on mutation.
    public bool HasNoDebtHistory => DebtEntries.Count == 0;

    public string Initials => Detail is null
        ? "?"
        : $"{FirstLetter(Detail.Client.Name)}{FirstLetter(Detail.Client.LastName)}";

    private static string FirstLetter(string value) =>
        string.IsNullOrEmpty(value) ? string.Empty : value[..1].ToUpperInvariant();

    public Task LoadAsync(Guid clientId) => SafeExecuteAsync(async () =>
    {
        var result = await _clientAppService.GetDetailAsync(clientId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Detail = result.Value;

        var debtResult = await _clientDebtAppService.GetHistoryAsync(clientId);
        if (debtResult.IsFailure)
        {
            SetError(debtResult.Error);
            return;
        }

        DebtEntries.Clear();
        foreach (var entry in debtResult.Value)
            DebtEntries.Add(entry);

        Balance = DebtEntries.Where(e => e.Type == DebtEntryType.Charge).Sum(e => e.Amount) -
                  DebtEntries.Where(e => e.Type == DebtEntryType.Payment).Sum(e => e.Amount);
        OnPropertyChanged(nameof(HasNoDebtHistory));
    });

    [RelayCommand]
    private async Task GoToAddChargeAsync()
    {
        if (Detail is null)
            return;

        await Shell.Current.GoToAsync("debt-charge-form", new Dictionary<string, object>
        {
            ["ClientId"] = Detail.Client.Id,
            ["ClientName"] = $"{Detail.Client.Name} {Detail.Client.LastName}"
        });
    }

    [RelayCommand]
    private async Task GoToAddPaymentAsync()
    {
        if (Detail is null)
            return;

        await Shell.Current.GoToAsync("debt-payment-form", new Dictionary<string, object>
        {
            ["ClientId"] = Detail.Client.Id,
            ["ClientName"] = $"{Detail.Client.Name} {Detail.Client.LastName}",
            ["Balance"] = Balance
        });
    }

    [RelayCommand]
    private Task ToggleFavoriteAsync() => SafeExecuteAsync(async () =>
    {
        if (Detail is null)
            return;

        var result = await _clientAppService.SetFavoriteAsync(Detail.Client.Id, !Detail.Client.IsFavorite);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        var refreshed = await _clientAppService.GetDetailAsync(Detail.Client.Id);
        if (refreshed.IsSuccess)
            Detail = refreshed.Value;
    });

    [RelayCommand]
    private void Call()
    {
        if (Detail is null)
            return;

        PhoneDialer.Default.Open(Detail.Client.Phone);
    }

    [RelayCommand]
    private async Task SendWhatsAppAsync()
    {
        if (Detail is null)
            return;

        // wa.me expects digits only (country code + number, no "+"/spaces/dashes).
        var digitsOnly = new string(Detail.Client.Phone.Where(char.IsDigit).ToArray());
        await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{digitsOnly}"));
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        if (Detail?.Client.Email is not { Length: > 0 } email)
            return;

        await Launcher.Default.OpenAsync(new Uri($"mailto:{email}"));
    }

    [RelayCommand]
    private Task DeleteAsync() => SafeExecuteAsync(async () =>
    {
        if (Detail is null)
            return;

        var result = await _clientAppService.DeleteAsync(Detail.Client.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Deleted = true;
    });
}
