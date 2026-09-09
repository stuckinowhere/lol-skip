using Unqueued.Services;

namespace Unqueued.Tests;

public sealed class FakeTimeProvider : TimeProvider
{
    public FakeTimeProvider(DateTimeOffset localNow)
    {
        LocalNow = localNow;
    }

    public DateTimeOffset LocalNow { get; set; }

    public override TimeZoneInfo LocalTimeZone { get; } = TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => LocalNow.ToUniversalTime();
}
