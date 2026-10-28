namespace MilkBean.Installer.Services;

public sealed class ThemeApplyOrchestrator
{
    private readonly IEnvironmentService _env;
    private readonly IThemeManifestService _manifest;
    private readonly IBackupService _backup;
    private readonly IWallpaperService _wallpaper;
    private readonly IRainmeterService _rainmeter;
    private readonly INotesService _notes;
    private readonly ILoggingService _log;

    public ThemeApplyOrchestrator(
        IEnvironmentService env,
        IThemeManifestService manifest,
        IBackupService backup,
        IWallpaperService wallpaper,
        IRainmeterService rainmeter,
        INotesService notes,
        ILoggingService log)
    {
        _env = env;
        _manifest = manifest;
        _backup = backup;
        _wallpaper = wallpaper;
        _rainmeter = rainmeter;
        _notes = notes;
        _log = log;
    }

    public async Task<ApplyResult> ApplyAsync(string themeRoot, ThemeApplyOptions options)
    {
        var result = new ApplyResult();
        string? backupId = null;
        WallpaperInfo? previousWallpaper = null;

        try
        {
            AppPaths.EnsureDirectories();

            var manifest = _manifest.Load(themeRoot);
            var validation = _manifest.Validate(manifest, themeRoot);
            result.ValidationWarnings.AddRange(validation.Warnings);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors.AddRange(validation.Errors);
                return result;
            }

            var envReport = _env.Check();
            if (envReport.BlockingIssues.Count > 0)
            {
                result.Success = false;
                result.Errors.AddRange(envReport.BlockingIssues);
                return result;
            }

            previousWallpaper = _wallpaper.GetCurrentWallpaper();
            backupId = await _backup.CreateBackupAsync(new ThemeApplyContext
            {
                ThemeId = manifest.ThemeId,
                ThemeVersion = manifest.Version,
                ApplyWallpaper = options.ApplyWallpaper,
                ApplyClock = options.ApplyClock,
                ApplyCalendar = options.ApplyCalendar,
                ApplyNotes = options.ApplyNotes
            }).ConfigureAwait(false);
            result.BackupId = backupId;

            if (options.ApplyWallpaper && manifest.Wallpaper != null)
            {
                var wpFull = ThemeManifestService.NormalizeUnderRoot(themeRoot, manifest.Wallpaper.Default);
                if (wpFull != null && File.Exists(wpFull))
                {
                    await _wallpaper.ApplyAsync(wpFull, manifest.Wallpaper.FitMode).ConfigureAwait(false);
                    result.Steps["wallpaper"] = "ok";
                }
                else
                {
                    result.Steps["wallpaper"] = "skipped-missing";
                    result.Warnings.Add("Wallpaper file missing, skipped");
                }
            }

            if ((options.ApplyClock || options.ApplyCalendar) && _rainmeter.IsAvailable())
            {
                var rmRoot = Path.Combine(themeRoot, "Rainmeter");
                await _rainmeter.DeploySkinsAsync(rmRoot).ConfigureAwait(false);
                if (options.ApplyClock)
                {
                    await _rainmeter.ActivateSkinAsync(@"MilkBean\Clock\Clock.ini").ConfigureAwait(false);
                    result.Steps["clock"] = "ok";
                }
                if (options.ApplyCalendar)
                {
                    await _rainmeter.ActivateSkinAsync(@"MilkBean\Calendar\Calendar.ini").ConfigureAwait(false);
                    result.Steps["calendar"] = "ok";
                }
            }
            else if (options.ApplyClock || options.ApplyCalendar)
            {
                result.Steps["rainmeter"] = "unavailable";
                result.Warnings.Add("Rainmeter not installed; clock/calendar not enabled");
            }

            if (options.ApplyNotes)
            {
                await _notes.EnsureStoreExistsAsync().ConfigureAwait(false);
                result.Steps["notes"] = "ok";
            }

            result.Success = true;
            result.Message = "Theme applied";
            _log.Info($"Apply success: {manifest.ThemeId}, backup={backupId}");
        }
        catch (Exception ex)
        {
            _log.Error("Apply failed, attempting rollback", ex);
            result.Success = false;
            result.Errors.Add(ex.Message);

            if (previousWallpaper != null)
            {
                try
                {
                    await _wallpaper.RestoreAsync(previousWallpaper).ConfigureAwait(false);
                    result.Warnings.Add("Wallpaper rolled back");
                }
                catch (Exception rex)
                {
                    _log.Error("Wallpaper rollback failed", rex);
                }
            }
        }

        return result;
    }
}

public sealed class ThemeApplyOptions
{
    public bool ApplyWallpaper { get; set; } = true;
    public bool ApplyClock { get; set; } = true;
    public bool ApplyCalendar { get; set; } = true;
    public bool ApplyNotes { get; set; } = true;
}

public sealed class ApplyResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? BackupId { get; set; }
    public Dictionary<string, string> Steps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
}
