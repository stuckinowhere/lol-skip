using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Unqueued.Services;

namespace Unqueued;

public partial class App : Application
{
    private AppSession? _session;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        AppIcons.ApplyTo(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _session = new AppSession(desktop);
            _session.ViewModel.PropertyChanged += (_, _) => SyncTrayPlay();
            _session.Start();
            SyncTrayPlay();
        }

        AppIcons.ApplyTo(this);
        AppIcons.RetryTray(this);
        base.OnFrameworkInitializationCompleted();
    }

    private void OnTrayClicked(object? sender, EventArgs e) => _session?.ToggleRitual();

    private void OnOpenRitual(object? sender, EventArgs e) => _session?.ShowRitual();

    private void OnSkipFromTray(object? sender, EventArgs e) => _session?.SkipFromTray();

    private void OnPlayFromTray(object? sender, EventArgs e) => _session?.PlayFromTray();

    private void OnQuit(object? sender, EventArgs e) => _session?.Quit();

    private void SyncTrayPlay()
    {
        var play = TrayIcon.GetIcons(this)?
            .SelectMany(icon => icon.Menu?.Items ?? [])
            .OfType<NativeMenuItem>()
            .FirstOrDefault(item => item.Header == "Play");
        if (play is null)
            return;

        play.IsEnabled = _session?.ViewModel.CanPlay == true;
    }
}
