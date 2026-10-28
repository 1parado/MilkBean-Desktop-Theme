namespace MilkBean.Installer.Services;

/// <summary>
/// 文件夹自定义图标应用与恢复。MVP 仅支持用户主动选择的文件夹。
/// </summary>
public interface IFolderIconService
{
    Task<FolderIconBackup> ApplyIconAsync(string folderPath, string icoPath);
    Task<RestoreResult> RestoreAsync(FolderIconBackup backup);
    void RefreshShellIcons();
}

public sealed class FolderIconBackup
{
    public string FolderPath { get; set; } = "";
    public string? OriginalIconPath { get; set; }
    public string? OriginalDesktopIniContent { get; set; }
    public string AppliedIconPath { get; set; } = "";
    public DateTimeOffset ModifiedAt { get; set; }
    public bool Success { get; set; }
}

public enum RestoreStatus
{
    Success,
    Skipped,
    Failed,
    NotSupported
}

public sealed class RestoreResult
{
    public RestoreStatus Status { get; set; }
    public string? Message { get; set; }
}
