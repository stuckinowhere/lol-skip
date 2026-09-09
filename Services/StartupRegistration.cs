using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace Unqueued.Services;

public static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovedRunKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string ApprovedStartupFolderKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";
    private const string ValueName = "WasdLolSkip";
    private const string ShortcutName = "wasdlol skip.lnk";
    private static readonly string[] LegacyRunNames = ["!WasdLolSkip", "Unqueued"];

    // Windows 11 Startup Apps: 0x02 = enabled.
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
        TryWriteRunKey(stable);
        TryWriteStartupShortcut(stable);
        TryWriteLogonTask(stable);
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

            key.SetValue(ValueName, $"\"{path}\"");
            WriteApproved(ApprovedRunKey, ValueName);
            WriteApproved(ApprovedRunKey, "!WasdLolSkip"); // leftover name, keep enabled if present
        }
        catch
        {
        }
    }

    private static void TryWriteStartupShortcut(string exePath)
    {
        try
        {
            var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            Directory.CreateDirectory(startup);
            var linkPath = Path.Combine(startup, ShortcutName);

            var type = Type.GetTypeFromProgID("WScript.Shell");
            if (type is null)
                return;

            var shell = Activator.CreateInstance(type);
            if (shell is null)
                return;

            var shortcut = type.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                null,
                shell,
                [linkPath]);
            if (shortcut is null)
                return;

            var shortcutType = shortcut.GetType();
            SetCom(shortcutType, shortcut, "TargetPath", exePath);
            SetCom(shortcutType, shortcut, "WorkingDirectory", Path.GetDirectoryName(exePath) ?? "");
            SetCom(shortcutType, shortcut, "WindowStyle", 1);
            SetCom(shortcutType, shortcut, "Description", "wasdlol skip");

            var ico = Path.Combine(Path.GetDirectoryName(exePath) ?? "", "WasdLolSkip.ico");
            SetCom(shortcutType, shortcut, "IconLocation", File.Exists(ico) ? ico : exePath);

            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            WriteApproved(ApprovedStartupFolderKey, ShortcutName);
        }
        catch
        {
        }
    }

    private static void TryWriteLogonTask(string path)
    {
        // ONLOGON tasks often need elevation on Windows 11; ignore failures.
        if (TrySchtasks(path, withDelay: true))
            return;
        TrySchtasks(path, withDelay: false);
    }

    private static bool TrySchtasks(string path, bool withDelay)
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
            psi.ArgumentList.Add("/Create");
            psi.ArgumentList.Add("/F");
            psi.ArgumentList.Add("/SC");
            psi.ArgumentList.Add("ONLOGON");
            psi.ArgumentList.Add("/IT");
            psi.ArgumentList.Add("/TN");
            psi.ArgumentList.Add("WasdLolSkip");
            psi.ArgumentList.Add("/TR");
            psi.ArgumentList.Add(path);
            if (withDelay)
            {
                psi.ArgumentList.Add("/DELAY");
                psi.ArgumentList.Add("0000:03");
            }

            using var process = Process.Start(psi);
            if (process is null)
                return false;

            process.WaitForExit(4000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void WriteApproved(string keyPath, string name)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath, true);
            key.SetValue(name, StartupEnabled, RegistryValueKind.Binary);
        }
        catch
        {
        }
    }

    private static void SetCom(Type type, object target, string property, object value) =>
        type.InvokeMember(property, BindingFlags.SetProperty, null, target, [value]);

    private static bool PathsEqual(string a, string b) =>
        string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
}
