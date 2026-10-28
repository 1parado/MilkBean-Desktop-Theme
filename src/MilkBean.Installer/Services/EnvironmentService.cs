using System.Runtime.InteropServices;

namespace MilkBean.Installer.Services;

public sealed class EnvironmentService : IEnvironmentService
{
    private readonly ILoggingService _log;

    public EnvironmentService(ILoggingService log) => _log = log;

    public EnvironmentReport Check()
    {
        var report = new EnvironmentReport
        {
            OsVersion = Environment.OSVersion.VersionString,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            IsSupportedOs = OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major >= 10,
            RainmeterInstalled = IsRainmeterInstalled(),
            RainmeterSkinsPath = GetRainmeterSkinsPath()
        };

        try
        {
            report.PrimaryScreenWidth = GetSystemMetrics(0);
            report.PrimaryScreenHeight = GetSystemMetrics(1);
        }
        catch (Exception ex)
        {
            _log.Warn($"Cannot read screen size: {ex.Message}");
            report.Warnings.Add("Cannot read primary screen resolution");
        }

        if (!report.IsSupportedOs)
            report.BlockingIssues.Add("Unsupported OS. Requires Windows 10/11 x64");

        if (!report.RainmeterInstalled)
            report.Warnings.Add("Rainmeter not detected. Clock and calendar require Rainmeter.");

        _log.Info($"Env check: OS={report.OsVersion}, Rainmeter={report.RainmeterInstalled}");
        return report;
    }

    public bool IsRainmeterInstalled()
    {
        var paths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rainmeter", "Rainmeter.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Rainmeter", "Rainmeter.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Rainmeter", "Rainmeter.exe")
        };
        return paths.Any(File.Exists);
    }

    public string? GetRainmeterSkinsPath()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var candidate = Path.Combine(docs, "Rainmeter", "Skins");
        if (Directory.Exists(candidate)) return candidate;
        var programSkins = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rainmeter", "Skins");
        return Directory.Exists(programSkins) ? programSkins : candidate;
    }

    public string GetAppDataRoot() => AppPaths.Root;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
