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

        if (Store.HasDecidedToday)
            Window.Opened += HideOnceOpened;

        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _clock.Tick += (_, _) => OnClock();
        _clock.Start();
    }

    public void ShowRitual()
    {
        if (Window is null)
            return;

        ViewModel.Refresh();
        Window.Show();
        Window.ShowInTaskbar = true;
        Window.WindowState = WindowState.Normal;
        Window.Activate();
        Window.Topmost = true;
    }

    public void HideToTray()
    {
        if (Window is null)
            return;

        Window.Hide();
        Window.ShowInTaskbar = false;
    }

    public void BeginPassFromTray()
    {
        ViewModel.BeginPassConfirm();
        ShowRitual();
    }

    public void Quit()
    {
        _exiting = true;
        _desktop.Shutdown();
    }

    public void Dispose()
    {
        _clock?.Stop();
        Blocker.Dispose();
    }

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
