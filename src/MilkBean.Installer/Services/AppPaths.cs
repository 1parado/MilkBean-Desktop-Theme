namespace MilkBean.Installer.Services;

/// <summary>
/// 统一管理本产品本地数据路径（用户目录，尽量不需管理员权限）。
/// </summary>
public static class AppPaths
{
    public const string AppFolderName = "MilkBean";

    public static string Root =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);

    public static string AppDir => Path.Combine(Root, "App");
    public static string ThemesDir => Path.Combine(Root, "Themes");
    public static string ConfigDir => Path.Combine(Root, "Config");
    public static string BackupsDir => Path.Combine(Root, "Backups");
    public static string LogsDir => Path.Combine(Root, "Logs");

    public static string NotesPath => Path.Combine(ConfigDir, "notes.json");
    public static string AppStatePath => Path.Combine(ConfigDir, "app-state.json");
    public static string UserPrefsPath => Path.Combine(ConfigDir, "user-preferences.json");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppDir);
        Directory.CreateDirectory(ThemesDir);
        Directory.CreateDirectory(ConfigDir);
        Directory.CreateDirectory(BackupsDir);
        Directory.CreateDirectory(LogsDir);
    }
}
