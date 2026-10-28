# Build MilkBean Desktop Theme (Windows)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build MilkBean.Desktop.Theme.sln -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$out = Join-Path $root "publish"
Write-Host "Publishing installer + notes to $out ..." -ForegroundColor Cyan
dotnet publish src/MilkBean.Installer/MilkBean.Installer.csproj -c Release -r win-x64 --self-contained false -o $out
dotnet publish src/MilkBean.Notes/MilkBean.Notes.csproj -c Release -r win-x64 --self-contained false -o $out

Write-Host "Done. Output: $out" -ForegroundColor Green
Write-Host "Run: $out\ApplyTheme.exe"
