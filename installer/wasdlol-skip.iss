; wasdlol skip — Windows installer
; Bundles the self-contained .NET 8 exe (no SDK / runtime install).
; Per-user, no admin. Installs to %LocalAppData%\WasdLolSkip.

#ifndef MyAppVersion
  #define MyAppVersion "1.2.1"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish\win-x64"
#endif

#define MyAppName "wasdlol skip"
#define MyAppPublisher "WASD"
#define MyAppURL "https://github.com/stuckinowhere/lol-skip"
#define MyAppExeName "WasdLolSkip.exe"

[Setup]
AppId={{8F3C1A2B-9D4E-4B67-A812-7C0E5D91F4B0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={localappdata}\WasdLolSkip
DisableDirPage=yes
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=..\artifacts
OutputBaseFilename=wasdlol-skip-v{#MyAppVersion}-win-x64-setup
SetupIconFile=..\Assets\unqueued.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
CloseApplications=force
CloseApplicationsFilter=WasdLolSkip.exe
RestartApplications=no
UsedUserAreasWarning=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Start {#MyAppName} when I sign in to Windows"; GroupDescription: "Additional options:"; Flags: checkedonce
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\WasdLolSkip.ico"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\WasdLolSkip.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\WasdLolSkip.ico"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "WasdLolSkip"; ValueData: """{app}\{#MyAppExeName}"" --startup"; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\WasdLolSkip.ico"
Type: dirifempty; Name: "{app}"

[Code]
procedure KillWasdLolSkip;
var
  ResultCode: Integer;
begin
  { CloseApplications applies to Setup only; tray apps survive uninstall without this. }
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM WasdLolSkip.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(800);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsWin64 then
  begin
    MsgBox('wasdlol skip requires 64-bit Windows 10 (1809 or later) or Windows 11.', mbError, MB_OK);
    Result := False;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  KillWasdLolSkip;
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  KillWasdLolSkip;
  RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'WasdLolSkip');
  Result := True;
end;
