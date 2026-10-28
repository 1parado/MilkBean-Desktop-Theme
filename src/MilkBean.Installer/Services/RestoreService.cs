using System.Text.Json;

namespace MilkBean.Installer.Services;

public sealed class RestoreService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IBackupService _backup;
    private readonly IWallpaperService _wallpaper;
    private readonly IRainmeterService _rainmeter;
    private readonly ILoggingService _log;

    public RestoreService(
        IBackupService backup,
        IWallpaperService wallpaper,
        IRainmeterService rainmeter,
        ILoggingService log)
    {
        _backup = backup;
        _wallpaper = wallpaper;
        _rainmeter = rainmeter;
        _log = log;
    }

    public async Task<RestoreReport> RestoreLatestAsync()
    {
        var list = await _backup.ListBackupsAsync().ConfigureAwait(false);
        if (list.Count == 0)
            return new RestoreReport { Success = false, Message = "No backup found" };
        return await RestoreAsync(list[0].BackupId).ConfigureAwait(false);
    }

    public async Task<RestoreReport> RestoreAsync(string backupId)
    {
        var report = new RestoreReport { BackupId = backupId };
        var dir = Path.Combine(AppPaths.BackupsDir, backupId);
        if (!Directory.Exists(dir))
        {
            report.Success = false;
            report.Message = "Backup directory missing";
            return report;
        }

        var wpFile = Path.Combine(dir, "wallpaper.json");
        if (File.Exists(wpFile))
        {
            try
            {
                var json = await File.ReadAllTextAsync(wpFile).ConfigureAwait(false);
                var info = JsonSerializer.Deserialize<WallpaperInfo>(json, JsonOptions);
                if (info != null)
                {
                    await _wallpaper.RestoreAsync(info).ConfigureAwait(false);
                    report.Items["wallpaper"] = RestoreStatus.Success;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Restore wallpaper failed", ex);
                report.Items["wallpaper"] = RestoreStatus.Failed;
            }
        }
        else
        {
            report.Items["wallpaper"] = RestoreStatus.Skipped;
        }

        try
        {
            if (_rainmeter.IsAvailable())
            {
                await _rainmeter.DeactivateAllMilkBeanSkinsAsync().ConfigureAwait(false);
                report.Items["rainmeter"] = RestoreStatus.Success;
            }
            else
            {
                report.Items["rainmeter"] = RestoreStatus.Skipped;
            }
        }
        catch (Exception ex)
        {
            _log.Error("Restore Rainmeter failed", ex);
            report.Items["rainmeter"] = RestoreStatus.Failed;
        }

        report.Items["folder-icons"] = RestoreStatus.Skipped;
        report.Success = report.Items.Values.All(s => s != RestoreStatus.Failed);
        report.Message = report.Success ? "Restore completed" : "Restore finished with failures; see log";
        _log.Info($"Restore: {report.Message}, backup={backupId}");
        return report;
    }
}

public sealed class RestoreReport
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? BackupId { get; set; }
    public Dictionary<string, RestoreStatus> Items { get; set; } = new();
}
