namespace MilkBean.Installer.Services;

/// <summary>
/// 壁纸读取、应用与恢复。实现时需验证多显示器与高 DPI 场景。
/// </summary>
public interface IWallpaperService
{
    WallpaperInfo GetCurrentWallpaper();
    Task ApplyAsync(string imagePath, string fitMode = "fill");
    Task RestoreAsync(WallpaperInfo backup);
}

public sealed class WallpaperInfo
{
    public string? Path { get; set; }
    public string? Style { get; set; }
    public bool MultiMonitor { get; set; }
    public Dictionary<string, string>? PerMonitorPaths { get; set; }
}
