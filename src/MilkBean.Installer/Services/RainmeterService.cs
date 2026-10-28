using System.Diagnostics;

namespace MilkBean.Installer.Services;

public sealed class RainmeterService : IRainmeterService
{
    private readonly IEnvironmentService _env;
    private readonly ILoggingService _log;

    public RainmeterService(IEnvironmentService env, ILoggingService log)
    {
        _env = env;
        _log = log;
    }

    public bool IsAvailable() => _env.IsRainmeterInstalled();

    public Task DeploySkinsAsync(string themeRainmeterRoot)
    {
        var skinsRoot = _env.GetRainmeterSkinsPath();
        if (string.IsNullOrEmpty(skinsRoot))
            throw new InvalidOperationException("Cannot determine Rainmeter Skins directory");

        Directory.CreateDirectory(skinsRoot);

        var sourceMilkBean = Path.Combine(themeRainmeterRoot, "Skins", "MilkBean");
        if (!Directory.Exists(sourceMilkBean))
            sourceMilkBean = Path.Combine(themeRainmeterRoot, "MilkBean");

        if (!Directory.Exists(sourceMilkBean))
            throw new DirectoryNotFoundException($"MilkBean skins not found under: {themeRainmeterRoot}");

        var dest = Path.Combine(skinsRoot, "MilkBean");
        CopyDirectory(sourceMilkBean, dest);
        _log.Info($"Deployed Rainmeter skins to: {dest}");
        return Task.CompletedTask;
    }

    public Task ActivateSkinAsync(string skinRelativePath)
    {
        return SendBangAsync($"!ActivateConfig \"{Path.GetDirectoryName(skinRelativePath)}\" \"{Path.GetFileName(skinRelativePath)}\"");
    }

    public Task DeactivateSkinAsync(string skinRelativePath)
    {
        var config = Path.GetDirectoryName(skinRelativePath)?.Replace('\\', '/') ?? skinRelativePath;
        return SendBangAsync($"!DeactivateConfig \"{config}\"");
    }

    public async Task DeactivateAllMilkBeanSkinsAsync()
    {
        await SendBangAsync("!DeactivateConfig MilkBean\\Clock").ConfigureAwait(false);
        await SendBangAsync("!DeactivateConfig MilkBean\\Calendar").ConfigureAwait(false);
        await SendBangAsync("!DeactivateConfig MilkBean\\Notes").ConfigureAwait(false);
        _log.Info("Requested deactivation of all MilkBean skins");
    }

    public Task<RainmeterStateSnapshot> CaptureStateAsync()
    {
        return Task.FromResult(new RainmeterStateSnapshot
        {
            SkinsRoot = _env.GetRainmeterSkinsPath(),
            CapturedAt = DateTimeOffset.Now,
            ActiveMilkBeanSkins = new List<string>()
        });
    }

    public Task RestoreStateAsync(RainmeterStateSnapshot snapshot) => DeactivateAllMilkBeanSkinsAsync();

    private async Task SendBangAsync(string bang)
    {
        var exe = FindRainmeterExe();
        if (exe == null)
        {
            _log.Warn("Rainmeter.exe not found");
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = bang,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p != null) await p.WaitForExitAsync().ConfigureAwait(false);
            _log.Info($"Rainmeter bang: {bang}");
        }
        catch (Exception ex)
        {
            _log.Error($"Rainmeter command failed: {bang}", ex);
        }
    }

    private static string? FindRainmeterExe()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rainmeter", "Rainmeter.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Rainmeter", "Rainmeter.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Rainmeter", "Rainmeter.exe")
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }
}
