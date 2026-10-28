using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MilkBean.Installer.Services;

public sealed class WallpaperService : IWallpaperService
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;

    private readonly ILoggingService _log;

    public WallpaperService(ILoggingService log) => _log = log;

    public WallpaperInfo GetCurrentWallpaper()
    {
        var info = new WallpaperInfo();
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
            if (key != null)
            {
                info.Path = key.GetValue("WallPaper") as string;
                var style = key.GetValue("WallpaperStyle") as string;
                var tile = key.GetValue("TileWallpaper") as string;
                info.Style = MapStyle(style, tile);
            }
        }
        catch (Exception ex)
        {
            _log.Warn($"Read wallpaper registry failed: {ex.Message}");
        }
        _log.Info($"Current wallpaper: {info.Path ?? "(none)"}, style={info.Style}");
        return info;
    }

    public Task ApplyAsync(string imagePath, string fitMode = "fill")
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Wallpaper file not found", imagePath);

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true);
            if (key != null)
            {
                var (style, tile) = MapFitMode(fitMode);
                key.SetValue("WallpaperStyle", style);
                key.SetValue("TileWallpaper", tile);
            }
        }
        catch (Exception ex)
        {
            _log.Warn($"Write wallpaper style failed: {ex.Message}");
        }

        var result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        if (!result)
        {
            var err = Marshal.GetLastWin32Error();
            _log.Error($"Set wallpaper failed, Win32Error={err}");
            throw new InvalidOperationException($"Set wallpaper failed (Win32: {err})");
        }

        _log.Info($"Wallpaper applied: {imagePath}, fitMode={fitMode}");
        return Task.CompletedTask;
    }

    public Task RestoreAsync(WallpaperInfo backup)
    {
        if (backup.Path != null && File.Exists(backup.Path))
            return ApplyAsync(backup.Path, backup.Style ?? "fill");

        _log.Warn("No valid wallpaper path in backup");
        return Task.CompletedTask;
    }

    private static string MapStyle(string? wallpaperStyle, string? tile)
    {
        if (tile == "1") return "tile";
        return wallpaperStyle switch
        {
            "0" => "center",
            "2" => "stretch",
            "6" => "fit",
            "10" => "fill",
            "22" => "span",
            _ => "fill"
        };
    }

    private static (string style, string tile) MapFitMode(string fitMode) => fitMode.ToLowerInvariant() switch
    {
        "center" => ("0", "0"),
        "tile" => ("0", "1"),
        "stretch" => ("2", "0"),
        "fit" => ("6", "0"),
        "span" => ("22", "0"),
        _ => ("10", "0")
    };

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
