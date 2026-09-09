using Unqueued.Services;

namespace Unqueued.Tests;

public class StreakStoreTests
{
    [Fact]
    public void FreshInstall_IsDayZero_Blocked_AndUndecided()
    {
        var (store, _) = NewStore(new DateTimeOffset(2026, 9, 9, 9, 0, 0, TimeSpan.Zero));
        store.LoadOrCreate();

        Assert.Equal(0, store.StreakDays);
        Assert.True(store.IsBlocked);
        Assert.False(store.IsAllowed);
        Assert.False(store.HasDecidedToday);
        Assert.Equal(new DateOnly(2026, 9, 9), store.State.InstallDate);
    }

    [Fact]
    public void CheckIn_KeepsBlocked_AndMarksDecided()
    {
        var (store, _) = NewStore(new DateTimeOffset(2026, 9, 9, 9, 0, 0, TimeSpan.Zero));
        store.LoadOrCreate();
        store.CheckIn();

        Assert.True(store.HasDecidedToday);
        Assert.True(store.IsBlocked);
        Assert.Equal(new DateOnly(2026, 9, 9), store.State.LastCheckInDate);
        Assert.Null(store.State.LastPassDate);
        Assert.Null(store.State.AllowedUntil);
    }

    [Fact]
    public void Pass_ResetsStreak_AndAllowsUntilLocalMidnight()
    {
        var (store, _) = NewStore(new DateTimeOffset(2026, 9, 9, 21, 15, 0, TimeSpan.Zero));
        store.LoadOrCreate();
        store.Pass();

        Assert.Equal(0, store.StreakDays);
        Assert.True(store.IsAllowed);
        Assert.False(store.IsBlocked);
        Assert.True(store.HasDecidedToday);
        Assert.Equal(new DateOnly(2026, 9, 9), store.State.LastPassDate);
        Assert.Equal(new DateTimeOffset(2026, 9, 10, 0, 0, 0, TimeSpan.Zero), store.State.AllowedUntil);
    }

    [Fact]
    public void NextDayAfterPass_BlocksAgain_StreakIsOne()
    {
        var start = new DateTimeOffset(2026, 9, 9, 21, 15, 0, TimeSpan.Zero);
        var (store, clock) = NewStore(start);
        store.LoadOrCreate();
        store.Pass();

        clock.LocalNow = new DateTimeOffset(2026, 9, 10, 0, 0, 1, TimeSpan.Zero);

        Assert.Equal(1, store.StreakDays);
        Assert.False(store.IsAllowed);
        Assert.True(store.IsBlocked);
        Assert.False(store.HasDecidedToday);
    }

    [Fact]
    public void FiveDaysWithoutPass_CountsFromInstall()
    {
        var (store, clock) = NewStore(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));
        store.LoadOrCreate();
        clock.LocalNow = new DateTimeOffset(2026, 9, 6, 8, 0, 0, TimeSpan.Zero);

        Assert.Equal(5, store.StreakDays);
        Assert.True(store.IsBlocked);
        Assert.False(store.HasDecidedToday);
    }

    [Fact]
    public void ReloadsPersistedPassFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), "unqueued-tests", Guid.NewGuid().ToString("n"), "state.json");
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero));
        var first = new StreakStore(clock, path);
        first.LoadOrCreate();
        first.Pass();

        var second = new StreakStore(clock, path);
        second.LoadOrCreate();

        Assert.True(second.IsAllowed);
        Assert.Equal(0, second.StreakDays);
        Assert.Equal(new DateOnly(2026, 9, 9), second.State.LastPassDate);
    }

    private static (StreakStore Store, FakeTimeProvider Clock) NewStore(DateTimeOffset now)
    {
        var path = Path.Combine(Path.GetTempPath(), "unqueued-tests", Guid.NewGuid().ToString("n"), "state.json");
        var clock = new FakeTimeProvider(now);
        return (new StreakStore(clock, path), clock);
    }
}
