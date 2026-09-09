# Unqueued

A small Windows 11 app that counts the days you are not playing League of Legends, starts with Windows, and keeps the League client closed until you choose otherwise.

At login it opens a compact ritual window: a gold day count on black, **Another day without LoL**, and **Pass**. Closing the window does not unlock League. Quit from the tray if you want to stop protection.

## What it does

- **Streak** — calendar days since your last Pass (or since first launch if you have never passed). After a Pass the number is 0 until the next day you stay clean.
- **Another day without LoL** — records today’s check-in, keeps League blocked, hides to the tray.
- **Pass** — confirms, then resets the streak to 0 and allows League until local midnight.
- **Already decided today** — starts in the tray. At midnight (or the next login) the ritual comes back and blocking resumes if a Pass has expired.
- **Startup** — on first launch of `Unqueued.exe`, writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. No administrator account required.
- **Blocking** — every second, while League is not allowed, it kills these processes if they appear:

  - `LeagueClient.exe`
  - `LeagueClientUx.exe`
  - `LeagueClientUxRender.exe`
  - `League of Legends.exe`

  Riot Client is left running so Valorant or TFT can still start. As soon as League’s own client launches, it is closed.

State is stored in `%AppData%\Unqueued\state.json`.

This is a same-user, no-driver block. It is not AppLocker. Closing Unqueued from the tray stops protection.

## Run on Windows 11

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build.

```bash
dotnet restore
dotnet run --project Unqueued.csproj
```

`dotnet run` skips startup-registry registration on purpose (it would point at the SDK host). Publish a Windows exe, then launch that exe once to register at login.

## Publish a self-contained exe

From the repo root:

```bash
dotnet publish Unqueued.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/win-x64
```

Copy the `publish/win-x64` folder to the PC that should run it and start `Unqueued.exe`. A Visual Studio publish profile lives at `Properties/PublishProfiles/win-x64.pubxml`.

There is no installer in this version. Zip the published folder if you want to move it.

## Tray

- **Open** — show the ritual window
- **Pass** — same confirm flow as the window
- **Quit — stops protection** — exits the app; League can run again

## Design

Monochrome LoL-adjacent chrome: client dark `#010A13`, bone `#F0E6D2`, gold `#C8AA6E`. Typefaces are Cinzel and IBM Plex Sans (SIL Open Font License). No Riot marks, crests, or client fonts.

## Linux / this repo

The UI is Avalonia, so `dotnet run` can open the ritual window on Linux. Process killing and the Run-key startup entry are Windows-only no-ops everywhere else.
