# wasdlol skip

<p align="left">
  <img src="Assets/unqueued.png" width="72" height="72" alt="wasdlol skip mark">
</p>

Count the days you are not playing League of Legends. **Skip** locks the client until tomorrow. **Play** lets Riot start.

Windows 11, gold-on-ink tray app. The day count is stored on this PC and survives restarts.

## Install

1. Download `wasdlol-skip-v*-win-x64.zip` from [Releases](https://github.com/stuckinowhere/lol-skip/releases).
2. Unzip and run `WasdLolSkip.exe`.
3. No .NET install is required — the zip is self-contained.

The first launch copies the app to `%LocalAppData%\WasdLolSkip` and adds one Windows startup entry so it can run at login, before a typical Riot Client start. You can turn that entry off in **Settings → Apps → Startup**.

## What it does

At login (or when you open the app) you get the day count.

| Action | Result |
| --- | --- |
| **Skip lol today** | Closes League / Riot Client / Vanguard tray, locks them until tomorrow, hides to the tray. Play is disabled for the rest of the day. |
| **Play** | Unlocks League until local midnight and starts Riot Client / Vanguard if they are set to launch with Windows. No extra confirm. |
| **Minimize / close** | Minimize stays in the taskbar. Close hides to the tray; protection keeps running. |
| **Quit** (tray) | Exits the app. League can run again. |

The streak lives in `%AppData%\Unqueued\state.json`.

## Tray

- **Open** — show the window
- **Skip lol today** / **Play** — same as the window (Play is disabled after a skip)
- **Quit — stops protection** — exits

## Build from source

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64), clone this repo, then:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\publish-windows.ps1
```

That writes `publish\win-x64\WasdLolSkip.exe` and launches it.

Dev loop:

```powershell
.\run-windows.ps1
```

`dotnet run` does **not** register startup (it would point Windows at the SDK, not the app). Use the published exe or a Release zip for daily use.

## Design

Ink `#010A13`, bone `#F0E6D2`, gold `#C8AA6E`. House brand is **WASD**. Reuse it from [`Brand/`](Brand/README.md). Cinzel + IBM Plex Sans (SIL OFL). Marks are original geometry, not Riot art.

## Cutting a release

On `main`:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions tests, publishes a self-contained win-x64 build, and opens a Release with `wasdlol-skip-v1.0.0-win-x64.zip` and `checksums.txt`.
