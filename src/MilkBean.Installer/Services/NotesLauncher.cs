using System.Diagnostics;

namespace MilkBean.Installer.Services;

public static class NotesLauncher
{
    public static bool TryLaunch(ILoggingService log)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "MilkBeanNotes.exe"),
            Path.Combine(baseDir, "..", "MilkBean.Notes", "MilkBeanNotes.exe"),
            Path.Combine(baseDir, "..", "..", "..", "..", "src", "MilkBean.Notes", "bin", "Debug", "net8.0-windows", "MilkBeanNotes.exe"),
            Path.Combine(baseDir, "..", "..", "..", "..", "src", "MilkBean.Notes", "bin", "Release", "net8.0-windows", "MilkBeanNotes.exe")
        };

        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (!File.Exists(full)) continue;
            try
            {
                Process.Start(new ProcessStartInfo { FileName = full, UseShellExecute = true });
                log.Info("Launched notes: " + full);
                return true;
            }
            catch (Exception ex) { log.Error("Launch notes failed", ex); }
        }

        log.Warn("MilkBeanNotes.exe not found");
        return false;
    }
}
