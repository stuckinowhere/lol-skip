using Microsoft.Win32;

namespace Unqueued.Services;

public static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Unqueued";

    public static void TryRegister()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var path = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path))
            return;

        // Skip `dotnet run` / `dotnet Unqueued.dll` so we do not register the SDK host.
        if (!path.EndsWith("Unqueued.exe", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKey, true);
            key.SetValue(ValueName, $"\"{path}\"");
        }
        catch
        {
            // Startup registration is best-effort; the app still runs without it.
        }
    }
}
