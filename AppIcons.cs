using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Unqueued;

internal static class AppIcons
{
    public static WindowIcon? Load()
    {
        try
        {
            var ico = NativeWindowIcon.FindSidecarIco();
            if (ico is not null)
            {
                using var file = File.OpenRead(ico);
                return new WindowIcon(file);
            }

            using var stream = AssetLoader.Open(new Uri("avares://WasdLolSkip/Assets/unqueued.png"));
            return new WindowIcon(stream);
        }
        catch
        {
            return null;
        }
    }

    public static void ApplyTo(Application app)
    {
        var icon = Load();
        if (icon is null)
            return;

        var trayIcons = TrayIcon.GetIcons(app);
        if (trayIcons is null)
            return;

        foreach (var tray in trayIcons)
            tray.Icon = icon;
    }

    public static void ApplyToWindow(Window window)
    {
        var icon = Load();
        if (icon is not null)
            window.Icon = icon;
    }
}
