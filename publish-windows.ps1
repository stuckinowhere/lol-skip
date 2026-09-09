# Build Unqueued.exe for this Windows PC, then open the output folder.
# Double-click Unqueued.exe there. First launch registers it at Windows login.

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Install the .NET 8 SDK, then run this again:"
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0"
    Start-Process "https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

$out = Join-Path $PSScriptRoot "publish\win-x64"
dotnet publish .\Unqueued.csproj -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=none `
    -o $out

$exe = Join-Path $out "Unqueued.exe"
Write-Host ""
Write-Host "Built: $exe"
Write-Host "Starting Unqueued..."
Start-Process $exe
Start-Process explorer.exe $out
