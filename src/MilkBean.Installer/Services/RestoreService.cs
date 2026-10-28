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
    private readonly IFolderIconService _folderIcons;
    private readonly ILoggingService _log;

    public RestoreService(
        IBackupService backup,
        IWallpaperService wallpaper,
        IRainmeterService rainmeter,
        IFolderIconService folderIcons,
        ILoggingService log)
    {
        _backup = backup;
        _wallpaper = wallpaper;
        _rainmeter = rainmeter;
        _folderIcons = folderIcons;
        _log = log;
    }

    public async Task<RestoreReport> RestoreLatestAsync()
    {
        var list = await _backup.ListBackupsAsync().ConfigureAwait(false);
        if (list.Count == 0)
            return new RestoreReport { Success = false, Message = "没有找到可用备份" };
        return await RestoreAsync(list[0].BackupId).ConfigureAwait(false);
    }

    public async Task<RestoreReport> RestoreAsync(string backupId)
    {
        var report = new RestoreReport { BackupId = backupId };
        var dir = Path.Combine(AppPaths.BackupsDir, backupId);
        if (!Directory.Exists(dir))
        {
            report.Success = false;
            report.Message = "备份目录不存在";
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
                _log.Error("恢复壁纸失败", ex);
                report.Items["wallpaper"] = RestoreStatus.Failed;
            }
        }
        else report.Items["wallpaper"] = RestoreStatus.Skipped;

        try
        {
            if (_rainmeter.IsAvailable())
            {
                await _rainmeter.DeactivateAllMilkBeanSkinsAsync().ConfigureAwait(false);
                report.Items["rainmeter"] = RestoreStatus.Success;
            }
            else report.Items["rainmeter"] = RestoreStatus.Skipped;
        }
        catch (Exception ex)
        {
            _log.Error("恢复 Rainmeter 失败", ex);
            report.Items["rainmeter"] = RestoreStatus.Failed;
        }

        report.Items["folder-icons"] = await RestoreFolderIconsAsync().ConfigureAwait(false);
        report.Success = report.Items.Values.All(s => s != RestoreStatus.Failed);
        report.Message = report.Success ? "恢复完成" : "恢复过程中存在失败项，请查看日志";
        _log.Info($"恢复结果: {report.Message}, backup={backupId}");
        return report;
    }

    private async Task<RestoreStatus> RestoreFolderIconsAsync()
    {
        var path = Path.Combine(AppPaths.ConfigDir, "folder-icons-applied.json");
        if (!File.Exists(path)) return RestoreStatus.Skipped;
        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var list = JsonSerializer.Deserialize<List<FolderIconBackup>>(json, JsonOptions) ?? new();
            if (list.Count == 0) return RestoreStatus.Skipped;

            var anyFailed = false;
            var anySuccess = false;
            foreach (var item in list)
            {
                var r = await _folderIcons.RestoreAsync(item).ConfigureAwait(false);
                if (r.Status == RestoreStatus.Failed) anyFailed = true;
                if (r.Status == RestoreStatus.Success) anySuccess = true;
            }
            if (anyFailed) return RestoreStatus.Failed;
            if (anySuccess) return RestoreStatus.Success;
            return RestoreStatus.Skipped;
        }
        catch (Exception ex)
        {
            _log.Error("恢复文件夹图标失败", ex);
            return RestoreStatus.Failed;
        }
    }
}

public sealed class RestoreReport
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? BackupId { get; set; }
    public Dictionary<string, RestoreStatus> Items { get; set; } = new();
}
