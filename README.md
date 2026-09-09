# wasdlol skip

A Windows 11 app you run **on your PC**. It counts days without League of Legends, starts at login (ahead of typical Riot Client startup), and keeps League closed unless you Play.

## Install

Download the latest **win-x64 zip** from [Releases](https://github.com/stuckinowhere/lol-skip/releases), unzip it, and run `WasdLolSkip.exe`. The first launch copies itself to `%LocalAppData%\WasdLolSkip` and registers one Windows startup entry.

A `v1.0.0` tag produces a GitHub Release with `wasdlol-skip-v1.0.0-win-x64.zip`.

## Build from source

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).
2. Get this repo onto the machine (clone, or download the ZIP and unzip it).
3. In that folder, either:

**Local publish — a real `WasdLolSkip.exe` that starts with Windows**

Right-click `publish-windows.ps1` → **Run with PowerShell**.  
Or from PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\publish-windows.ps1
```

That builds `publish\win-x64\WasdLolSkip.exe` and launches it. The first time it starts, it copies itself to `%LocalAppData%\WasdLolSkip` and writes one HKCU Run entry (`--startup`) so it can run before Riot Client. It does not add a scheduled task or Startup-folder shortcut.

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

## License

[MIT](LICENSE). Cinzel and IBM Plex Sans are under the [SIL Open Font License](Assets/Fonts/LICENSE.txt).

This is an unofficial tool. League of Legends, Riot Client, and Vanguard are trademarks of Riot Games, Inc. This project is not affiliated with, endorsed, or sponsored by Riot Games.

## Cutting a release

After this branch is on `main`:

```powershell
git checkout main
git pull
git tag v1.0.0
git push origin v1.0.0
```

That runs `.github/workflows/release.yml`, which tests, publishes a self-contained `win-x64` exe, and creates a GitHub Release with `wasdlol-skip-v1.0.0-win-x64.zip`. Later versions are `v1.0.1`, `v1.1.0`, and so on.
