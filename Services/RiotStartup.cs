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

    internal static bool LooksLikeRiot(string name, string command)
    {
        if (!TrySplitCommand(command, out var file, out _))
            return false;

        if (!RiotPaths.IsTrustedExecutable(file))
            return false;

        return ContainsNeedle(name) || ContainsNeedle(file);
    }

    private static bool ContainsNeedle(string text) =>
        Needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static void TryStart(string command)
    {
        if (!TrySplitCommand(command, out var file, out var arguments))
            return;

        if (!File.Exists(file))
            return;

        if (!RiotPaths.IsTrustedExecutable(file))
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

    internal static bool TrySplitCommand(string command, out string file, out string arguments)
    {
        file = string.Empty;
        arguments = string.Empty;
        command = command.Trim();
        if (command.Length == 0)
            return false;

        if (command.StartsWith('"'))
        {
            var end = command.IndexOf('"', 1);
            if (end <= 1)
                return false;

            file = command[1..end];
            arguments = command[(end + 1)..].Trim();
            return file.Length > 0;
        }

        var exe = command.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exe >= 0)
        {
            var fileEnd = exe + 4;
            file = command[..fileEnd];
            arguments = fileEnd < command.Length ? command[fileEnd..].Trim() : string.Empty;
            return file.Length > 0;
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
