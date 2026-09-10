# wasdlol skip

A Windows 11 app you run **on your PC**. It counts days without League of Legends, starts at login (ahead of typical Riot Client startup), and keeps League closed unless you Play.

## Install

Download **`wasdlol-skip-*-win-x64-setup.exe`** from [Releases](https://github.com/stuckinowhere/lol-skip/releases) and run it.

The installer is per-user (no admin prompt). It includes the .NET runtime, so you do **not** install the .NET SDK. It places the app in `%LocalAppData%\WasdLolSkip`, adds a Start Menu shortcut, and can start at Windows sign-in. Uninstall from **Settings → Apps**.

The installed app checks [GitHub Releases](https://github.com/stuckinowhere/lol-skip/releases) for a newer version when it starts. If one exists, it offers **Download** (the setup exe). You can also choose **Check for updates** from the tray menu. It does not install the update by itself — run the new setup; uninstall/upgrade will close the running copy first.

64-bit Windows 10 (1809 or later) or Windows 11.

A portable zip (`wasdlol-skip-*-win-x64.zip`) is also on each release if you would rather not use Setup.

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

That builds `publish\win-x64\WasdLolSkip.exe` and launches it. The first time it starts, it copies itself to `%LocalAppData%\WasdLolSkip` **only if that exe is not already there**, then writes one HKCU Run entry (`--startup`) so it can run before Riot Client. It will not overwrite an existing installed copy. It does not add a scheduled task or Startup-folder shortcut.

**Dev loop**

```powershell
.\run-windows.ps1
```

`dotnet run` does **not** add itself to startup (it would register the SDK, not the app). Use the published exe for the real daily setup.

If PowerShell blocks scripts: `Set-ExecutionPolicy -Scope Process Bypass`.

## What it does

At login you get a gold-on-black window with the day count.

- **Skip lol today** — close League / Riot Client processes and lock them until tomorrow. Skip and Play stay visible but disabled for the rest of the local day.
- **Play** — no extra confirm. Allows League, and starts Riot Client / Vanguard if they are set to launch at Windows logon.
- Close the window to the tray. **Quit** from the tray stops protection.

While locked it kills League and Riot Client processes if they are already running.

The day count lives in `%AppData%\Unqueued\state.json` and **survives restarts**.

## Tray

- **Open** — show the window
- **Skip lol today** / **Play** — same as the window
- **Check for updates** — ask GitHub if a newer setup is published
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

That runs `.github/workflows/release.yml`, which tests, publishes a self-contained win-x64 build, and creates a GitHub Release with `wasdlol-skip-v1.0.0-win-x64-setup.exe` (installer) and `wasdlol-skip-v1.0.0-win-x64.zip`. Later versions are `v1.0.1`, `v1.1.0`, and so on.

To build the installer locally (needs [Inno Setup 6](https://jrsoftware.org/isinfo.php)):

```powershell
.\installer\build-installer.ps1
```
