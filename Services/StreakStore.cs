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
    private readonly object _gate = new();

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

    public bool IsAllowed
    {
        get
        {
            lock (_gate)
                return State.AllowedUntil is { } until && _time.GetLocalNow() < until;
        }
    }

    public bool IsBlocked => !IsAllowed;

    public bool HasSkippedToday
    {
        get
        {
            lock (_gate)
                return State.LastSkipDate == Today;
        }
    }

    public bool CanPlay => !HasSkippedToday && !IsAllowed;

    public bool HasDecidedToday
    {
        get
        {
            lock (_gate)
                return State.LastSkipDate == Today || State.LastPassDate == Today;
        }
    }

    public int StreakDays
    {
        get
        {
            lock (_gate)
            {
                if (State.LastPassDate is { } pass)
                    return Math.Max(0, Today.DayNumber - pass.DayNumber);

                if (State.InstallDate == default)
                    return 0;

                return Math.Max(0, Today.DayNumber - State.InstallDate.DayNumber);
            }
        }
    }

    public DateOnly StreakOrigin
    {
        get
        {
            lock (_gate)
                return State.LastPassDate?.AddDays(1) ?? State.InstallDate;
        }
    }

    public void LoadOrCreate()
    {
        lock (_gate)
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
                SaveUnlocked();
            }
        }
    }

    public void CheckIn()
    {
        Skip();
    }

    public void Skip()
    {
        lock (_gate)
        {
            State.LastSkipDate = Today;
            State.LastCheckInDate = Today;
            State.AllowedUntil = null;
            SaveUnlocked();
        }

        Changed?.Invoke();
    }

    public void Pass()
    {
        lock (_gate)
        {
            if (State.LastSkipDate == Today)
                return;

            var local = _time.GetLocalNow();
            var midnight = new DateTimeOffset(local.Date.AddDays(1), local.Offset);
            State.LastPassDate = Today;
            State.LastCheckInDate = Today;
            State.AllowedUntil = midnight;
            SaveUnlocked();
        }

        Changed?.Invoke();
    }

    public void NotifyClock()
    {
        bool expired;
        lock (_gate)
            expired = State.AllowedUntil is { } until && _time.GetLocalNow() >= until;

        if (expired)
            Changed?.Invoke();
    }

    public void Save()
    {
        lock (_gate)
            SaveUnlocked();
    }

    private void SaveUnlocked()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(State, JsonOptions);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(_path))
            File.Replace(tmp, _path, destinationBackupFileName: null);
        else
            File.Move(tmp, _path);
    }
}
