# Decode base64 theme assets (Windows)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
python scripts/decode_assets.py
if ($LASTEXITCODE -ne 0) {
  Write-Host "Python decode failed; ensure Python 3 is installed." -ForegroundColor Yellow
  exit $LASTEXITCODE
}
