using Unqueued.Services;

namespace Unqueued.Tests;

public class ProcessBlockerTests
{
    [Fact]
    public void WatchesLeagueRiotAndVanguardTrayProcesses()
    {
        Assert.Equal(
        [
            "LeagueClient",
            "LeagueClientUx",
            "LeagueClientUxRender",
            "League of Legends",
            "LoLPatcher",
            "LeagueCrashHandler",
            "LoLCrashHandler",
            "RiotClientServices",
            "RiotClientUx",
            "RiotClientUxRender",
            "RiotClientCrashHandler",
            "vgtray"
        ], ProcessBlocker.ProcessNames);
    }

    [Fact]
    public void RejectsNamedRiotFolderUnderDownloads()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var decoy = Path.Combine(home, "Downloads", "Riot Games", "evil.exe");
        Assert.False(RiotPaths.IsTrustedExecutable(decoy));
    }

    [Fact]
    public void StartOnLinux_IsNoOp()
    {
        if (OperatingSystem.IsWindows())
            return;

        using var blocker = new ProcessBlocker(() => true);
        blocker.Start();
        blocker.Stop();
    }

    [Theory]
    [InlineData(@"C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe", true)]
    [InlineData(@"C:\Program Files\Riot Vanguard\vgtray.exe", true)]
    [InlineData(@"D:\Riot Games\League of Legends\LeagueClient.exe", true)]
    [InlineData(@"C:\Windows\System32\notepad.exe", false)]
    [InlineData(@"C:\Riot Games\..\Windows\System32\notepad.exe", false)]
    public void OnlyKillsExecutablesUnderRiotFolders(string path, bool trusted) =>
        Assert.Equal(trusted, RiotPaths.IsTrustedExecutable(path));
}
