using Unqueued.Services;

namespace Unqueued.Tests;

public class RiotStartupTests
{
    [Fact]
    public void SplitsQuotedCommandAndArgs()
    {
        Assert.True(RiotStartup.TrySplitCommand(
            @" ""C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe"" --launch-product=league_of_legends ",
            out var file,
            out var arguments));

        Assert.Equal(@"C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe", file);
        Assert.Equal("--launch-product=league_of_legends", arguments);
    }

    [Fact]
    public void SplitsUnquotedProgramFilesPathOnExe()
    {
        Assert.True(RiotStartup.TrySplitCommand(
            @"C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe --launch-product=league_of_legends",
            out var file,
            out var arguments));

        Assert.Equal(@"C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe", file);
        Assert.Equal("--launch-product=league_of_legends", arguments);
    }

    [Fact]
    public void SplitsBareExeWithoutArgs()
    {
        Assert.True(RiotStartup.TrySplitCommand(@"C:\Games\RiotClientServices.exe", out var file, out var arguments));
        Assert.Equal(@"C:\Games\RiotClientServices.exe", file);
        Assert.Equal(string.Empty, arguments);
    }

    [Theory]
    [InlineData("Riot Client", @"C:\Windows\notepad.exe", true)]
    [InlineData("Steam", @"C:\Program Files\Steam\steam.exe", false)]
    [InlineData("Helper", @"C:\Program Files\Riot Games\Riot Client\RiotClientServices.exe", true)]
    public void LooksLikeRiot_UsesNameOrExecutable(string name, string command, bool expected) =>
        Assert.Equal(expected, RiotStartup.LooksLikeRiot(name, command));
}
