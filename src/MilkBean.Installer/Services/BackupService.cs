using System.Text.Json;

namespace MilkBean.Installer.Services;

public sealed class BackupService : IBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILoggingService _log;
    private readonly IWallpaperService _wallpaper;

    public BackupService(ILoggingService log, IWallpaperService wallpaper)
    {
        _log = log;
        _wallpaper = wallpaper;
    }

    public string GetBackupRoot() => AppPaths.BackupsDir;

    public async Task<string> CreateBackupAsync(ThemeApplyContext context)
    {
        AppPaths.EnsureDirectories();
        var id = $"backup-{DateTime.Now:yyyyMMdd-HHmmss}";
        var dir = Path.Combine(AppPaths.BackupsDir, id);
        Directory.CreateDirectory(dir);

        var manifest = new BackupManifest
        {
            BackupId = id,
            CreatedAt = DateTimeOffset.Now,
            ProductVersion = "1.0.0",
            ThemeId = context.ThemeId,
            ItemStatus = new Dictionary<string, string>()
        };

        try
        {
            var wp = _wallpaper.GetCurrentWallpaper();
            var wpPath = Path.Combine(dir, "wallpaper.json");
            await File.WriteAllTextAsync(wpPath, JsonSerializer.Serialize(wp, JsonOptions)).ConfigureAwait(false);
            manifest.ItemStatus["wallpaper"] = "ok";
        }
        catch (Exception ex)
        {
            _log.Error("Backup wallpaper failed", ex);
            manifest.ItemStatus["wallpaper"] = "failed";
        }

        manifest.ItemStatus["rainmeter"] = "pending";
        manifest.ItemStatus["folder-icons"] = "pending";

        var manifestPath = Path.Combine(dir, "manifest.json");
        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions)).ConfigureAwait(false);
        _log.Info($"Backup created: {id}");
        return id;
    }

    public async Task<BackupManifest?> GetManifestAsync(string backupId)
    {
        var path = Path.Combine(AppPaths.BackupsDir, backupId, "manifest.json");
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<BackupManifest>(json, JsonOptions);
    }

    public async Task<IReadOnlyList<BackupManifest>> ListBackupsAsync()
    {
        AppPaths.EnsureDirectories();
        var list = new List<BackupManifest>();
        foreach (var dir in Directory.GetDirectories(AppPaths.BackupsDir))
        {
            var id = Path.GetFileName(dir);
            var m = await GetManifestAsync(id).ConfigureAwait(false);
            if (m != null) list.Add(m);
        }
        return list.OrderByDescending(x => x.CreatedAt).ToList();
    }
}
