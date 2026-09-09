using System.Text.Json;
using Unqueued.Models;

namespace Unqueued.Services;

public sealed class StreakStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly TimeProvider _time;
    private readonly string _path;

    public StreakStore(TimeProvider? time = null, string? path = null)
    {
        _time = time ?? TimeProvider.System;
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Unqueued",
            "state.json");
        State = new AppState();
    }

    public AppState State { get; private set; }

    public event Action? Changed;

    public DateTime Now => _time.GetLocalNow().LocalDateTime;

    public DateOnly Today => DateOnly.FromDateTime(Now);

    public bool IsAllowed =>
        State.AllowedUntil is { } until && _time.GetLocalNow() < until;

    public bool IsBlocked => !IsAllowed;

    public bool HasDecidedToday =>
        State.LastCheckInDate == Today || State.LastPassDate == Today;

    public int StreakDays
    {
        get
        {
            if (State.LastPassDate is { } pass)
                return Math.Max(0, Today.DayNumber - pass.DayNumber);

            if (State.InstallDate == default)
                return 0;

            return Math.Max(0, Today.DayNumber - State.InstallDate.DayNumber);
        }
    }

    public DateOnly StreakOrigin => State.LastPassDate?.AddDays(1) ?? State.InstallDate;

    public void LoadOrCreate()
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                State = JsonSerializer.Deserialize<AppState>(json, JsonOptions) ?? new AppState();
            }
        }
        catch
        {
            State = new AppState();
        }

        if (State.InstallDate == default)
        {
            State.InstallDate = Today;
            Save();
        }
    }

    public void CheckIn()
    {
        State.LastCheckInDate = Today;
        Save();
        Changed?.Invoke();
    }

    public void Pass()
    {
        var local = _time.GetLocalNow();
        var midnight = new DateTimeOffset(local.Date.AddDays(1), local.Offset);
        State.LastPassDate = Today;
        State.LastCheckInDate = Today;
        State.AllowedUntil = midnight;
        Save();
        Changed?.Invoke();
    }

    public void NotifyClock()
    {
        if (State.AllowedUntil is { } until && _time.GetLocalNow() >= until)
            Changed?.Invoke();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(State, JsonOptions);
        File.WriteAllText(_path, json);
    }
}
