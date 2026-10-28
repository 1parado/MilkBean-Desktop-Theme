namespace MilkBean.Installer.Services;

/// <summary>
/// Detect Windows version, architecture, display settings and Rainmeter.
/// </summary>
public interface IEnvironmentService
{
    EnvironmentReport Check();
    bool IsRainmeterInstalled();
    string? GetRainmeterSkinsPath();
    string GetAppDataRoot();
}

public sealed class EnvironmentReport
{
    public string OsVersion { get; set; } = "";
    public string Architecture { get; set; } = "";
    public bool IsSupportedOs { get; set; }
    public int PrimaryScreenWidth { get; set; }
    public int PrimaryScreenHeight { get; set; }
    public double DpiScale { get; set; } = 1.0;
    public bool RainmeterInstalled { get; set; }
    public string? RainmeterVersion { get; set; }
    public string? RainmeterSkinsPath { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> BlockingIssues { get; set; } = new();
}
