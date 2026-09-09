# Unqueued

A Windows 11 app you run **on your PC**. It counts days without League of Legends, starts at login, and closes the League client unless you Pass.

This repo is the source. The cloud session only compiled it. Blocking, startup, and the ritual window all run locally on Windows 11.

## Run it on your Windows 11 PC

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).
2. Get this repo onto the machine (clone, or download the ZIP and unzip it).
3. In that folder, either:

**Easiest — a real `Unqueued.exe` that starts with Windows**

Right-click `publish-windows.ps1` → **Run with PowerShell**.  
Or from PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\publish-windows.ps1
```

That builds `publish\win-x64\Unqueued.exe`, launches it, and opens the folder. Pin or leave that exe where you want it. The first time `Unqueued.exe` starts, it registers itself at login.

**Dev loop**

```powershell
.\run-windows.ps1
```

`dotnet run` does **not** add itself to startup (it would register the SDK, not the app). Use the published exe for the real daily setup.

If PowerShell blocks scripts: `Set-ExecutionPolicy -Scope Process Bypass`.

## What it does

At login you get a small gold-on-black window with the day count.

- **Another day without LoL** — keep League blocked, hide to the tray.
- **Pass** — reset the streak to 0 and allow League until local midnight.
- Closing the window still blocks. **Quit** from the tray stops protection.

While blocked it kills `LeagueClient.exe`, `LeagueClientUx.exe`, `LeagueClientUxRender.exe`, and `League of Legends.exe`. Riot Client is left running so Valorant/TFT still work.

State lives in `%AppData%\Unqueued\state.json`.

## Tray

- **Open** — show the ritual window
- **Pass** — same confirm flow as the window
- **Quit — stops protection** — exits; League can run again

## Design

[Palette 8528](https://www.color-hex.com/color-palette/8528): mint `#0AC8B9`, teal `#0397AB`, deep `#005A82`, panel `#0A323C`, ink `#091428`.

House brand is **WASD** (hex + WASD keys). Reuse it in other apps from [`Brand/`](Brand/README.md) — copy `wasd-mark.svg` / `.png` / `.ico` and `Controls/WasdMark.cs`. Cinzel + IBM Plex Sans (SIL OFL). Lane/shield marks are original geometry, not Riot art.
