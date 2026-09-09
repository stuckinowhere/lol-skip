namespace Unqueued.Models;

public sealed class AppState
{
    public DateOnly InstallDate { get; set; }
    public DateOnly? LastPassDate { get; set; }
    public DateOnly? LastCheckInDate { get; set; }
    public DateTimeOffset? AllowedUntil { get; set; }
}
