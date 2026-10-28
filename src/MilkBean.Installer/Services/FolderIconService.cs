using System.Runtime.InteropServices;
using System.Text;

namespace MilkBean.Installer.Services;

public sealed class FolderIconService : IFolderIconService
{
    private readonly ILoggingService _log;

    public FolderIconService(ILoggingService log) => _log = log;

    public async Task<FolderIconBackup> ApplyIconAsync(string folderPath, string icoPath)
    {
        var backup = new FolderIconBackup
        {
            FolderPath = Path.GetFullPath(folderPath),
            AppliedIconPath = Path.GetFullPath(icoPath),
            ModifiedAt = DateTimeOffset.Now
        };

        if (!Directory.Exists(backup.FolderPath))
        {
            backup.Success = false;
            _log.Error($"Folder not found: {backup.FolderPath}");
            return backup;
        }

        if (!File.Exists(backup.AppliedIconPath))
        {
            backup.Success = false;
            _log.Error($"Icon not found: {backup.AppliedIconPath}");
            return backup;
        }

        var iniPath = Path.Combine(backup.FolderPath, "desktop.ini");

        try
        {
            if (File.Exists(iniPath))
                backup.OriginalDesktopIniContent = await File.ReadAllTextAsync(iniPath).ConfigureAwait(false);

            var content = new StringBuilder();
            content.AppendLine("[.ShellClassInfo]");
            content.AppendLine($"IconResource={backup.AppliedIconPath},0");
            content.AppendLine("[ViewState]");
            content.AppendLine("Mode=");
            content.AppendLine("Vid=");
            content.AppendLine("FolderType=Generic");

            var dirInfo = new DirectoryInfo(backup.FolderPath);
            dirInfo.Attributes |= FileAttributes.System;

            await File.WriteAllTextAsync(iniPath, content.ToString(), Encoding.Unicode).ConfigureAwait(false);
            File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);

            RefreshShellIcons();
            backup.Success = true;
            _log.Info($"Folder icon applied: {backup.FolderPath}");
        }
        catch (UnauthorizedAccessException ex)
        {
            backup.Success = false;
            _log.Error($"No permission for folder icon: {backup.FolderPath}", ex);
        }
        catch (Exception ex)
        {
            backup.Success = false;
            _log.Error($"Apply folder icon failed: {backup.FolderPath}", ex);
        }

        return backup;
    }

    public async Task<RestoreResult> RestoreAsync(FolderIconBackup backup)
    {
        if (!Directory.Exists(backup.FolderPath))
            return new RestoreResult { Status = RestoreStatus.Skipped, Message = "Folder no longer exists" };

        var iniPath = Path.Combine(backup.FolderPath, "desktop.ini");

        try
        {
            if (File.Exists(iniPath))
            {
                var current = await File.ReadAllTextAsync(iniPath).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(backup.AppliedIconPath) &&
                    !current.Contains(backup.AppliedIconPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new RestoreResult
                    {
                        Status = RestoreStatus.Skipped,
                        Message = "User changed icon manually; skip restore"
                    };
                }
            }

            if (backup.OriginalDesktopIniContent != null)
            {
                await File.WriteAllTextAsync(iniPath, backup.OriginalDesktopIniContent, Encoding.Unicode).ConfigureAwait(false);
                File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);
            }
            else if (File.Exists(iniPath))
            {
                File.SetAttributes(iniPath, FileAttributes.Normal);
                File.Delete(iniPath);
            }

            RefreshShellIcons();
            _log.Info($"Folder icon restored: {backup.FolderPath}");
            return new RestoreResult { Status = RestoreStatus.Success };
        }
        catch (Exception ex)
        {
            _log.Error($"Restore folder icon failed: {backup.FolderPath}", ex);
            return new RestoreResult { Status = RestoreStatus.Failed, Message = ex.Message };
        }
    }

    public void RefreshShellIcons()
    {
        try
        {
            SHChangeNotify(0x08000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            _log.Warn($"Refresh shell icons failed: {ex.Message}");
        }
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
}
