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
    public void StartOnLinux_IsNoOp()
    {
        if (OperatingSystem.IsWindows())
            return;

        using var blocker = new ProcessBlocker(() => true);
        blocker.Start();
        blocker.Stop();
    }
}
