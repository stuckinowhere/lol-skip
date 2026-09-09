using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Unqueued.Services;

namespace Unqueued.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly StreakStore _store;
    private readonly AppSession? _session;

    public MainViewModel() : this(new StreakStore(), null)
    {
        // Design-time constructor.
    }

    public MainViewModel(StreakStore store, AppSession? session)
    {
        _store = store;
        _session = session;
        _store.Changed += Refresh;
        Refresh();
    }

    [ObservableProperty] private int _streakDays;
    [ObservableProperty] private string _eyebrow = "DAYS WITHOUT LEAGUE";
    [ObservableProperty] private string _statusLine = "Day 0 — the queue is closed.";
    [ObservableProperty] private string _detailLine = string.Empty;
    [ObservableProperty] private string _primaryLabel = "Another day without LoL";
    [ObservableProperty] private bool _isAllowed;
    [ObservableProperty] private bool _isConfirming;
    [ObservableProperty] private string _footer = "Closing keeps League blocked. Quit from the tray to stop protection.";

    [RelayCommand]
    private void Stay()
    {
        if (IsConfirming)
            return;

        if (!_store.IsAllowed)
            _store.CheckIn();

        Refresh();
        _session?.HideToTray();
    }

    [RelayCommand]
    public void BeginPassConfirm()
    {
        if (_store.IsAllowed)
            return;

        IsConfirming = true;
    }

    [RelayCommand]
    private void CancelPass()
    {
        IsConfirming = false;
    }

    [RelayCommand]
    private void ConfirmPass()
    {
        _store.Pass();
        IsConfirming = false;
        Refresh();
        _session?.HideToTray();
    }

    [RelayCommand]
    private void CloseToTray() => _session?.HideToTray();

    public void Refresh()
    {
        StreakDays = _store.StreakDays;
        IsAllowed = _store.IsAllowed;

        if (IsAllowed)
        {
            Eyebrow = "STREAK RESET";
            StatusLine = "Streak reset. League is allowed until midnight.";
            DetailLine = $"Allowed until {_store.State.AllowedUntil:t}";
            PrimaryLabel = "Back to tray";
            Footer = "League can run until local midnight. The ritual returns tomorrow.";
        }
        else if (StreakDays == 0)
        {
            Eyebrow = "DAYS WITHOUT LEAGUE";
            StatusLine = "Day 0 — the queue is closed.";
            DetailLine = "The client stays closed until you Pass.";
            PrimaryLabel = "Another day without LoL";
            Footer = "Closing keeps League blocked. Quit from the tray to stop protection.";
        }
        else
        {
            Eyebrow = "DAYS WITHOUT LEAGUE";
            StatusLine = "The queue is closed.";
            DetailLine = $"Since {_store.StreakOrigin:MMMM d, yyyy}";
            PrimaryLabel = "Another day without LoL";
            Footer = "Closing keeps League blocked. Quit from the tray to stop protection.";
        }
    }
}
