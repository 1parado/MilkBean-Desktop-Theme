namespace MilkBean.Installer.Services;

/// <summary>
/// 备份与恢复协调。每次首次应用主题生成独立备份目录。
/// </summary>
public interface IBackupService
{
    Task<string> CreateBackupAsync(ThemeApplyContext context);
    Task<BackupManifest?> GetManifestAsync(string backupId);
    Task<IReadOnlyList<BackupManifest>> ListBackupsAsync();
    string GetBackupRoot();
}

public sealed class ThemeApplyContext
{
    public string ThemeId { get; set; } = "";
    public string ThemeVersion { get; set; } = "";
    public bool ApplyWallpaper { get; set; } = true;
    public bool ApplyClock { get; set; } = true;
    public bool ApplyCalendar { get; set; } = true;
    public bool ApplyNotes { get; set; } = true;
}

public sealed class BackupManifest
{
    public string BackupId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string ProductVersion { get; set; } = "";
    public string ThemeId { get; set; } = "";
    public Dictionary<string, string> ItemStatus { get; set; } = new();
}
