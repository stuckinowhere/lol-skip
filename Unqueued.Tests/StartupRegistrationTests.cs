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
