using System.Diagnostics;
using Microsoft.Win32;

namespace Unqueued.Services;

public static class RiotStartup
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private static readonly string[] Needles =
    [
        "riotclient",
        "riot client",
        "riot games",
        "vgtray",
        "vanguard",
        "leagueclient"
    ];

    public static void LaunchConfiguredClients()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            if (key is null)
                return;

            foreach (var name in key.GetValueNames())
            {
                if (name.Contains("WasdLolSkip", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Unqueued", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = key.GetValue(name)?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (!LooksLikeRiot(name, value))
                    continue;

                TryStart(value);
            }
        }
        catch
        {
            // Best-effort: Play still unblocks even if a client cannot be relaunched.
        }
    }

    private static bool LooksLikeRiot(string name, string command) =>
        Needles.Any(needle =>
            name.Contains(needle, StringComparison.OrdinalIgnoreCase)
            || command.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static void TryStart(string command)
    {
        if (!TrySplitCommand(command, out var file, out var arguments))
            return;

        if (!File.Exists(file))
            return;

        var exeName = Path.GetFileNameWithoutExtension(file);
        try
        {
            if (Process.GetProcessesByName(exeName).Length > 0)
                return;
        }
        catch
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = true
            })?.Dispose();
        }
        catch
        {
            // Ignore launch failures; the user can start Riot from the shortcut.
        }
    }

    private static bool TrySplitCommand(string command, out string file, out string arguments)
    {
        file = string.Empty;
        arguments = string.Empty;
        command = command.Trim();
        if (command.Length == 0)
            return false;

        if (command.StartsWith('"'))
        {
            var end = command.IndexOf('"', 1);
            if (end < 0)
                return false;

            file = command[1..end];
            arguments = command[(end + 1)..].Trim();
            return true;
        }

        var space = command.IndexOf(' ');
        if (space < 0)
        {
            file = command;
            return true;
        }

        file = command[..space];
        arguments = command[(space + 1)..].Trim();
        return true;
    }
}
