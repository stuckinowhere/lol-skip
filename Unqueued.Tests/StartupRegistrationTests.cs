using Unqueued;
using Unqueued.Services;

namespace Unqueued.Tests;

public class StartupRegistrationTests
{
    [Fact]
    public void MissingApprovalIsNotAUserDisable() =>
        Assert.False(StartupRegistration.IsUserDisabled(null));

    [Fact]
    public void EnabledApprovalByteIsNotDisabled() =>
        Assert.False(StartupRegistration.IsUserDisabled(new byte[] { 0x02, 0x00, 0x00, 0x00 }));

    [Fact]
    public void DisabledApprovalByteIsRespected() =>
        Assert.True(StartupRegistration.IsUserDisabled(new byte[] { 0x03, 0x00, 0x00, 0x00 }));

    [Fact]
    public void CopyIfMissing_DoesNotOverwriteExistingDest()
    {
        var dir = Path.Combine(Path.GetTempPath(), "unqueued-tests", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(dir);
        var running = Path.Combine(dir, "running.exe");
        var dest = Path.Combine(dir, "WasdLolSkip.exe");
        File.WriteAllText(running, "new");
        File.WriteAllText(dest, "installed");

        Assert.False(StartupRegistration.CopyIfMissing(running, dest));
        Assert.Equal("installed", File.ReadAllText(dest));
    }

    [Fact]
    public void CopyIfMissing_CopiesWhenDestIsAbsent()
    {
        var dir = Path.Combine(Path.GetTempPath(), "unqueued-tests", Guid.NewGuid().ToString("n"));
        var running = Path.Combine(dir, "running.exe");
        var dest = Path.Combine(dir, "stable", "WasdLolSkip.exe");
        Directory.CreateDirectory(dir);
        File.WriteAllText(running, "new");

        Assert.True(StartupRegistration.CopyIfMissing(running, dest));
        Assert.Equal("new", File.ReadAllText(dest));
    }
}

public class SingleInstanceTests
{
    [Fact]
    public void StartupFlagIsDetected()
    {
        Assert.True(Program.IsStartupLaunch(["--startup"]));
        Assert.True(Program.IsStartupLaunch(["--Startup"]));
        Assert.False(Program.IsStartupLaunch([]));
        Assert.False(Program.IsStartupLaunch(["--other"]));
    }
}
