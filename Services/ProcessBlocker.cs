using System.Diagnostics;

namespace Unqueued.Services;

public sealed class ProcessBlocker : IDisposable
{
    public static readonly string[] ProcessNames =
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
    ];

    private readonly Func<bool> _shouldBlock;
    private Timer? _timer;

    public ProcessBlocker(Func<bool> shouldBlock)
    {
        _shouldBlock = shouldBlock;
    }

    public void Start()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (_shouldBlock())
            KillMatching();

        _timer ??= new Timer(_ => Tick(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void KillMatching()
    {
        if (!OperatingSystem.IsWindows())
            return;

        foreach (var name in ProcessNames)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(name);
            }
            catch
            {
                continue;
            }

            foreach (var process in processes)
            {
                try
                {
                    string? image;
                    try
                    {
                        image = process.MainModule?.FileName;
                    }
                    catch
                    {
                        continue;
                    }

                    if (!RiotPaths.IsTrustedExecutable(image))
                        continue;

                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Process may have already exited.
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
    }

    public void Dispose() => Stop();

    private void Tick()
    {
        if (!_shouldBlock())
            return;

        KillMatching();
    }
}
