using Avalonia;
using System;
using System.Runtime.Versioning;
using System.Threading;

namespace Unqueued;

internal static class SingleInstance
{
    public const string MutexName = "Unqueued.SingleInstance";
    public const string ShowEventName = "Unqueued.ShowExisting";
    public static EventWaitHandle? ShowEvent;
}

sealed class Program
{
    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {
        NativeWindowIcon.SetProcessAppId();

        _mutex = new Mutex(true, SingleInstance.MutexName, out var createdNew);
        if (!createdNew)
        {
            if (OperatingSystem.IsWindows())
                TryShowExisting();
            return;
        }

        SingleInstance.ShowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, SingleInstance.ShowEventName);

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            SingleInstance.ShowEvent.Dispose();
            SingleInstance.ShowEvent = null;
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }

    [SupportedOSPlatform("windows")]
    private static void TryShowExisting()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            using var show = EventWaitHandle.OpenExisting(SingleInstance.ShowEventName);
            show.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .LogToTrace();
}
