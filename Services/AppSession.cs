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
        ViewModel.SkipTodayCommand.Execute(null);
    }

    public void PlayFromTray()
    {
        ViewModel.PlayCommand.Execute(null);
    }

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
