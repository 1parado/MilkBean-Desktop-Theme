namespace MilkBean.Installer.Services;

public sealed class LoggingService : ILoggingService
{
    private readonly object _lock = new();
    private readonly string _logPath;

    public LoggingService()
    {
        AppPaths.EnsureDirectories();
        var name = $"install-{DateTime.Now:yyyyMMdd-HHmmss}.log";
        _logPath = Path.Combine(AppPaths.LogsDir, name);
    }

    public string GetLogDirectory() => AppPaths.LogsDir;
    public string GetLatestLogPath() => _logPath;

    public void Info(string message) => Write("INFO", message, null);
    public void Warn(string message) => Write("WARN", message, null);
    public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}";
        if (ex != null)
            line += Environment.NewLine + ex;

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logPath, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
