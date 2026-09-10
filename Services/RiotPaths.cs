namespace Unqueued.Services;

internal static class RiotPaths
{
    internal static bool IsTrustedExecutable(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string full;
        try
        {
            full = Path.GetFullPath(path);
        }
        catch
        {
            return false;
        }

        if (!full.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            return false;

        if (IsLooseUserFolder(full))
            return false;

        return HasDirectory(full, "Riot Games") || HasDirectory(full, "Riot Vanguard");
    }

    private static bool IsLooseUserFolder(string full)
    {
        foreach (var folder in LooseUserFolders())
        {
            if (string.IsNullOrWhiteSpace(folder))
                continue;

            string root;
            try
            {
                root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
            }
            catch
            {
                continue;
            }

            if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static IEnumerable<string> LooseUserFolders()
    {
        yield return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) is { Length: > 0 } home
            ? Path.Combine(home, "Downloads")
            : "";
        yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        yield return Path.GetTempPath();
    }

    private static bool HasDirectory(string full, string directoryName)
    {
        var parts = full.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].Equals(directoryName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
