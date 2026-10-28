# Changelog

## [1.0.0-mvp] - 2026-10-28

### Added
- Complete PRD / TECH_SPEC / ARCHITECTURE / TEST_PLAN
- theme.json and Rainmeter Clock/Calendar skins
- WPF installer wizard (Welcome / Check / Options / Result)
- Core services: Environment, Wallpaper, FolderIcon, Rainmeter, Notes, Backup, Restore, ThemeApplyOrchestrator, Logging
- AppPaths local data layout under %LOCALAPPDATA%\MilkBean

### Build
```powershell
dotnet build MilkBean.Desktop.Theme.sln -c Release
dotnet run --project src/MilkBean.Installer
```
