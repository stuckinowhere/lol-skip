using Avalonia;
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
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _session = new AppSession(desktop);
            _session.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnTrayClicked(object? sender, EventArgs e) => _session?.ShowRitual();

    private void OnOpenRitual(object? sender, EventArgs e) => _session?.ShowRitual();

    private void OnPassFromTray(object? sender, EventArgs e) => _session?.BeginPassFromTray();

    private void OnQuit(object? sender, EventArgs e) => _session?.Quit();
}
