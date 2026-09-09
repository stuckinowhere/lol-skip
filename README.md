# wasdlol skip

A Windows 11 app you run **on your PC**. It counts days without League of Legends, starts at login (ahead of typical Riot Client startup), and keeps League closed unless you Play.

## Run it on your Windows 11 PC

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).
2. Get this repo onto the machine (clone, or download the ZIP and unzip it).
3. In that folder, either:

**Easiest — a real `WasdLolSkip.exe` that starts with Windows**

Right-click `publish-windows.ps1` → **Run with PowerShell**.  
Or from PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\publish-windows.ps1
```

That builds `publish\win-x64\WasdLolSkip.exe`, launches it, and opens the folder. Pin or leave that exe where you want it. The first time it starts, it registers a logon task plus a startup entry so it can run before Riot Client.

**Dev loop**

```powershell
.\run-windows.ps1
```

`dotnet run` does **not** add itself to startup (it would register the SDK, not the app). Use the published exe for the real daily setup.

If PowerShell blocks scripts: `Set-ExecutionPolicy -Scope Process Bypass`.

## What it does

At login you get a gold-on-black window with the day count.

- **Skip lol today** — close League / Riot Client processes, lock them until tomorrow, hide to the tray. Play is disabled for the rest of the day.
- **Play** — no extra confirm. Allows League, and starts Riot Client / Vanguard if they are set to launch at Windows logon.
- There is no close button. **Quit** from the tray stops protection.

While locked it kills League and Riot Client processes if they are already running.

The day count lives in `%AppData%\Unqueued\state.json` and **survives restarts**.

## Tray

- **Open** — show the window
- **Skip lol today** / **Play** — same as the window
- **Quit — stops protection** — exits; League can run again

## Design

Client dark `#010A13`, bone `#F0E6D2`, gold `#C8AA6E`. House brand is **WASD** (hex + WASD keys). Reuse it from [`Brand/`](Brand/README.md). Cinzel + IBM Plex Sans (SIL OFL). Lane marks are original geometry, not Riot art.
