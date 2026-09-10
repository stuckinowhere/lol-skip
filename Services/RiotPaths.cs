namespace Unqueued.Services;

internal static class RiotPaths
{
    private static readonly string[] BlockedDirectories =
    [
        "Downloads",
        "Desktop",
        "Temp"
    ];

    internal static bool IsTrustedExecutable(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = SplitAndCollapse(path);
        if (parts.Count < 2)
            return false;

        var directories = parts.Take(parts.Count - 1).ToArray();
        if (directories.Any(IsBlockedDirectory))
            return false;

        return directories.Any(directory =>
            directory.Equals("Riot Games", StringComparison.OrdinalIgnoreCase)
            || directory.Equals("Riot Vanguard", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBlockedDirectory(string name) =>
        BlockedDirectories.Any(blocked => name.Equals(blocked, StringComparison.OrdinalIgnoreCase));

    private static List<string> SplitAndCollapse(string path)
    {
        var parts = path.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        var stack = new List<string>();
        foreach (var part in parts)
        {
            if (part is ".")
                continue;
            if (part is "..")
            {
                if (stack.Count > 0)
                    stack.RemoveAt(stack.Count - 1);
                continue;
            }

            stack.Add(part);
        }

        return stack;
    }
}
