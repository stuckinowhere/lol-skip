using System.Diagnostics;
using Microsoft.Win32;

namespace Unqueued.Services;

public static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovedRunKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string ApprovedStartupFolderKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";
    private const string ValueName = "WasdLolSkip";
    private const string ShortcutName = "wasdlol skip.lnk";
    private const string TaskName = "WasdLolSkip";
    private static readonly string[] LegacyRunNames = ["!WasdLolSkip", "Unqueued"];

    // Windows 11 Startup Apps: 0x02 = enabled, 0x03 = disabled by the user.
    private static readonly byte[] StartupEnabled =
        [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    public static void TryRegister()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var path = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path))
            return;

        var fileName = Path.GetFileName(path);
        if (!fileName.Equals("WasdLolSkip.exe", StringComparison.OrdinalIgnoreCase)
            && !fileName.Equals("Unqueued.exe", StringComparison.OrdinalIgnoreCase))
            return;

        var stable = EnsureLocalCopy(path);
        RemoveLeftoverLaunchers();
        TryWriteRunKey(stable);
    }

    private static string EnsureLocalCopy(string runningPath)
    {
        try
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WasdLolSkip");
            Directory.CreateDirectory(dir);

            var dest = Path.Combine(dir, "WasdLolSkip.exe");
            if (!PathsEqual(runningPath, dest))
            {
                try
                {
                    File.Copy(runningPath, dest, overwrite: true);
                }
                catch (IOException)
                {
                    if (!File.Exists(dest))
                        return runningPath;
                }

                var icoSource = Path.Combine(Path.GetDirectoryName(runningPath) ?? "", "WasdLolSkip.ico");
                if (!File.Exists(icoSource))
                    icoSource = Path.Combine(Path.GetDirectoryName(runningPath) ?? "", "unqueued.ico");
                if (File.Exists(icoSource))
                {
                    try
                    {
                        File.Copy(icoSource, Path.Combine(dir, "WasdLolSkip.ico"), overwrite: true);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            return File.Exists(dest) ? dest : runningPath;
        }
        catch
        {
            return runningPath;
        }
    }

    private static void TryWriteRunKey(string path)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKey, true);

            foreach (var legacy in LegacyRunNames)
                key.DeleteValue(legacy, throwOnMissingValue: false);

            key.SetValue(ValueName, $"\"{path}\" --startup");
            WriteApprovedIfEnabled(ApprovedRunKey, ValueName);
        }
        catch
        {
        }
    }

    private static void RemoveLeftoverLaunchers()
    {
        try
        {
            var linkPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                ShortcutName);
            if (File.Exists(linkPath))
                File.Delete(linkPath);
        }
        catch
        {
        }

        try
        {
            using var approved = Registry.CurrentUser.OpenSubKey(ApprovedStartupFolderKey, writable: true);
            approved?.DeleteValue(ShortcutName, throwOnMissingValue: false);
        }
        catch
        {
        }

        try
        {
            using var approved = Registry.CurrentUser.OpenSubKey(ApprovedRunKey, writable: true);
            approved?.DeleteValue("!WasdLolSkip", throwOnMissingValue: false);
        }
        catch
        {
        }

        TryDeleteLogonTask();
    }

    private static void TryDeleteLogonTask()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.ArgumentList.Add("/Delete");
            psi.ArgumentList.Add("/F");
            psi.ArgumentList.Add("/TN");
            psi.ArgumentList.Add(TaskName);

            using var process = Process.Start(psi);
            process?.WaitForExit(4000);
        }
        catch
        {
        }
    }

    private static void WriteApprovedIfEnabled(string keyPath, string name)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath, true);
            if (IsUserDisabled(key.GetValue(name)))
                return;

            key.SetValue(name, StartupEnabled, RegistryValueKind.Binary);
        }
        catch
        {
        }
    }

    internal static bool IsUserDisabled(object? value) =>
        value is byte[] { Length: > 0 } bytes && bytes[0] != 0x02;

    private static bool PathsEqual(string a, string b) =>
        string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
}
