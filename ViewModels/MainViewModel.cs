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
    [ObservableProperty] private string _eyebrow = "DAYS WITHOUT LOL";
    [ObservableProperty] private bool _canPlay;

    [RelayCommand]
    private void SkipToday()
    {
        _store.Skip();
        _session?.LockLeague();
        Refresh();
        _session?.HideToTray();
    }

    [RelayCommand]
    private void Play()
    {
        if (!_store.CanPlay)
            return;

        _store.Pass();
        Refresh();
        _session?.AllowLeague();
        _session?.HideToTray();
    }

    [RelayCommand]
    private void CloseToTray() => _session?.HideToTray();

    public void Refresh()
    {
        StreakDays = _store.StreakDays;
        CanPlay = _store.CanPlay;
        Eyebrow = _store.IsAllowed ? "PLAYING TODAY" : "DAYS WITHOUT LOL";
    }
}
