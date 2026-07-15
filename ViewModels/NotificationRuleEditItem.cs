using CommunityToolkit.Mvvm.ComponentModel;

namespace Beauty_Salon.ViewModels;

public sealed partial class NotificationRuleEditItem : ObservableObject
{
    public required int MinutesBefore { get; init; }
    public required string Label { get; init; }

    [ObservableProperty]
    private bool isEnabled;
}
