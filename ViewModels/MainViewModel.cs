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
    [ObservableProperty] private bool _canSkip = true;

    [RelayCommand(CanExecute = nameof(CanSkipToday))]
    private void SkipToday()
    {
        if (!CanSkipToday())
            return;

        _store.Skip();
        _session?.LockLeague();
        Refresh();
    }

    [RelayCommand(CanExecute = nameof(CanPlayToday))]
    private void Play()
    {
        if (!CanPlayToday())
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
        CanSkip = CanSkipToday();
        Eyebrow = _store.HasSkippedToday
            ? "LOCKED UNTIL TOMORROW"
            : _store.IsAllowed
                ? "PLAYING TODAY"
                : "DAYS WITHOUT LOL";
        SkipTodayCommand.NotifyCanExecuteChanged();
        PlayCommand.NotifyCanExecuteChanged();
    }

    private bool CanSkipToday() => !_store.HasSkippedToday;

    private bool CanPlayToday() => _store.CanPlay;
}
