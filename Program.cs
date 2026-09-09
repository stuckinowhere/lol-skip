using Avalonia;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;

namespace Unqueued;

internal static class SingleInstance
{
    public const string MutexName = "Unqueued.SingleInstance";
    public const string ShowEventName = "Unqueued.ShowExisting";
    public const string StartupArgument = "--startup";
    public static EventWaitHandle? ShowEvent;
}

sealed class Program
{
    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {
        NativeWindowIcon.SetProcessAppId();

        _mutex = new Mutex(false, SingleInstance.MutexName);
        bool createdNew;
        try
        {
            createdNew = _mutex.WaitOne(TimeSpan.Zero);
        }
        catch (AbandonedMutexException)
        {
            createdNew = true;
        }

        if (!createdNew)
        {
            if (OperatingSystem.IsWindows() && !IsStartupLaunch(args))
                TryShowExisting();
            _mutex.Dispose();
            _mutex = null;
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

    internal static bool IsStartupLaunch(string[] args) =>
        args.Any(argument =>
            string.Equals(argument, SingleInstance.StartupArgument, StringComparison.OrdinalIgnoreCase));

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
            .LogToTrace();
}
