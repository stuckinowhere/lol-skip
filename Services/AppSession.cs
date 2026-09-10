using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Unqueued.ViewModels;
using Unqueued.Views;

namespace Unqueued.Services;

public sealed class AppSession : IDisposable
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private DispatcherTimer? _clock;
    private Thread? _showListener;
    private volatile bool _listenForShow;
    private DateOnly _lastSeenDate;
    private bool _exiting;
    private readonly GitHubUpdateClient _updates = new();
    private int _checkingUpdates;
    private UpdateWindow? _updateWindow;

    public AppSession(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
        Store = new StreakStore();
        Store.LoadOrCreate();
        Blocker = new ProcessBlocker(() => Store.IsBlocked);
        ViewModel = new MainViewModel(Store, this);
        _lastSeenDate = Store.Today;
    }

    public StreakStore Store { get; }

    public ProcessBlocker Blocker { get; }

    public MainViewModel ViewModel { get; }

    public MainWindow? Window { get; private set; }

    public void Start()
    {
        StartupRegistration.TryRegister();
        Blocker.Start();

        _desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _desktop.Exit += (_, _) => Dispose();

        Window = new MainWindow
        {
            DataContext = ViewModel
        };
        Window.Closing += OnWindowClosing;
        _desktop.MainWindow = Window;

        if (Store.HasDecidedToday && LaunchedAtWindowsSignIn())
            Window.Opened += HideOnceOpened;

        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _clock.Tick += (_, _) => OnClock();
        _clock.Start();

        _listenForShow = true;
        _showListener = new Thread(ListenForShowRequests)
        {
            IsBackground = true,
            Name = "Unqueued.ShowListener"
        };
        _showListener.Start();

        Dispatcher.UIThread.Post(() => _ = CheckForUpdatesAsync(notifyWhenCurrent: false));
    }

    public void ShowRitual()
    {
        if (Window is null)
            return;

        ViewModel.Refresh();
        Window.Show();
        Window.ShowInTaskbar = true;
        Window.WindowState = WindowState.Normal;
        NativeWindowIcon.Apply(Window);
        Window.Activate();
        Window.Topmost = true;
    }

    public void ToggleRitual()
    {
        if (Window?.IsVisible == true)
            HideToTray();
        else
            ShowRitual();
    }

    public void HideToTray()
    {
        if (Window is null)
            return;

        Window.Hide();
        Window.ShowInTaskbar = false;
    }

    public void LockLeague() => Blocker.KillMatching();

    public void AllowLeague() => RiotStartup.LaunchConfiguredClients();

    public void SkipFromTray()
    {
        if (ViewModel.SkipTodayCommand.CanExecute(null))
            ViewModel.SkipTodayCommand.Execute(null);
    }

    public void PlayFromTray()
    {
        if (ViewModel.PlayCommand.CanExecute(null))
            ViewModel.PlayCommand.Execute(null);
    }

    public void CheckForUpdatesFromTray() => _ = CheckForUpdatesAsync(notifyWhenCurrent: true);

    public void Quit()
    {
        _exiting = true;
        _desktop.Shutdown();
    }

    public void Dispose()
    {
        _listenForShow = false;
        SingleInstance.ShowEvent?.Set();
        _clock?.Stop();
        Blocker.Dispose();
        _updates.Dispose();
    }

    private async Task CheckForUpdatesAsync(bool notifyWhenCurrent)
    {
        if (Interlocked.CompareExchange(ref _checkingUpdates, 1, 0) != 0)
            return;

        try
        {
            var result = await _updates.CheckAsync().ConfigureAwait(true);
            if (_exiting)
                return;

            if (result.Status == UpdateCheckStatus.Current && !notifyWhenCurrent)
                return;

            await Dispatcher.UIThread.InvokeAsync(() => ShowUpdateResult(result));
        }
        catch
        {
            if (!notifyWhenCurrent)
                return;

            await Dispatcher.UIThread.InvokeAsync(() => ShowUpdateResult(new UpdateCheckResult(
                UpdateCheckStatus.Failed,
                AppVersion.Current,
                null,
                null,
                null,
                null,
                "Could not check for updates.")));
        }
        finally
        {
            Interlocked.Exchange(ref _checkingUpdates, 0);
        }
    }

    private void ShowUpdateResult(UpdateCheckResult result)
    {
        if (_updateWindow is { IsVisible: true })
        {
            _updateWindow.Activate();
            return;
        }

        _updateWindow = new UpdateWindow(result);
        _updateWindow.Closed += (_, _) => _updateWindow = null;
        _updateWindow.Show();
        NativeWindowIcon.Apply(_updateWindow);
        _updateWindow.Activate();
    }

    private void ListenForShowRequests()
    {
        var showEvent = SingleInstance.ShowEvent;
        if (showEvent is null)
            return;

        while (_listenForShow)
        {
            if (showEvent.WaitOne(TimeSpan.FromMilliseconds(500)))
                Dispatcher.UIThread.Post(ShowRitual);
        }
    }

    private static bool LaunchedAtWindowsSignIn() =>
        Environment.GetCommandLineArgs().Any(argument =>
            string.Equals(argument, SingleInstance.StartupArgument, StringComparison.OrdinalIgnoreCase));

    private void HideOnceOpened(object? sender, EventArgs e)
    {
        if (Window is not null)
            Window.Opened -= HideOnceOpened;
        HideToTray();
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_exiting)
            return;

        e.Cancel = true;
        HideToTray();
    }

    private void OnClock()
    {
        var today = Store.Today;
        Store.NotifyClock();
        ViewModel.Refresh();

        if (today == _lastSeenDate)
            return;

        _lastSeenDate = today;
        if (!Store.HasDecidedToday)
            ShowRitual();
    }
}
