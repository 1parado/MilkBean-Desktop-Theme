using System.IO;
using System.Text;
using System.Windows;
using MilkBean.Installer.Services;
using MilkBean.Installer.Views;

namespace MilkBean.Installer;

public partial class MainWindow : Window
{
    private enum Step { Welcome, Check, Options, Result }

    private Step _step = Step.Welcome;
    private readonly ILoggingService _log;
    private readonly IEnvironmentService _env;
    private readonly IThemeManifestService _manifest;
    private readonly IWallpaperService _wallpaper;
    private readonly IRainmeterService _rainmeter;
    private readonly INotesService _notes;
    private readonly IBackupService _backup;
    private readonly IFolderIconService _folderIcons;
    private readonly ThemeApplyOrchestrator _apply;
    private readonly RestoreService _restore;
    private EnvironmentReport? _lastEnv;
    private string? _lastThemeRoot;

    public MainWindow()
    {
        InitializeComponent();

        _log = new LoggingService();
        _env = new EnvironmentService(_log);
        _manifest = new ThemeManifestService(_log);
        _wallpaper = new WallpaperService(_log);
        _rainmeter = new RainmeterService(_env, _log);
        _notes = new NotesService(_log);
        _backup = new BackupService(_log, _wallpaper);
        _folderIcons = new FolderIconService(_log);
        _apply = new ThemeApplyOrchestrator(_env, _manifest, _backup, _wallpaper, _rainmeter, _notes, _log);
        _restore = new RestoreService(_backup, _wallpaper, _rainmeter, _folderIcons, _log);

        _log.Info("Installer started");
    }

    private void BtnPrimary_Click(object sender, RoutedEventArgs e)
    {
        switch (_step)
        {
            case Step.Welcome: GoToCheck(); break;
            case Step.Check: GoToOptions(); break;
            case Step.Options: _ = RunApplyAsync(); break;
            case Step.Result: Close(); break;
        }
    }

    private void BtnSecondary_Click(object sender, RoutedEventArgs e)
    {
        if (_step == Step.Welcome)
        {
            MessageBox.Show(
                "Install guide:\n\n1. Wallpaper is backed up before apply.\n2. Clock/calendar need Rainmeter.\n3. Notes stay local only.\n4. You can restore anytime.\n\nLog dir:\n" + AppPaths.LogsDir,
                "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (_step == Step.Check) GoToCheck();
        else if (_step == Step.Options) GoToCheck();
        else if (_step == Step.Result)
            MessageBox.Show("Log file:\n" + _log.GetLatestLogPath(), "Diagnostics");
    }

    private void BtnFolderIcon_Click(object sender, RoutedEventArgs e)
    {
        var themeRoot = _lastThemeRoot ?? ResolveThemeRoot();
        if (themeRoot == null)
        {
            MessageBox.Show("Theme folder not found.", "Info");
            return;
        }
        var win = new FolderIconWindow(_folderIcons, _log, themeRoot) { Owner = this };
        win.ShowDialog();
    }

    private void BtnNotes_Click(object sender, RoutedEventArgs e)
    {
        if (!NotesLauncher.TryLaunch(_log))
        {
            MessageBox.Show(
                "MilkBeanNotes.exe not found.\nBuild src/MilkBean.Notes or place the exe next to the installer.",
                "Open Notes");
        }
    }

    private async void BtnRestore_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "Restore wallpaper, close MilkBean widgets, and restore recorded folder icons?",
            "Restore", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        BtnPrimary.IsEnabled = false;
        BtnRestore.IsEnabled = false;
        SubtitleText.Text = "Restoring...";

        try
        {
            var report = await _restore.RestoreLatestAsync().ConfigureAwait(true);
            var sb = new StringBuilder();
            sb.AppendLine(report.Message);
            foreach (var kv in report.Items)
                sb.AppendLine($"- {kv.Key}: {kv.Value}");
            MessageBox.Show(sb.ToString(), "Restore result");
        }
        catch (Exception ex)
        {
            _log.Error("Restore failed", ex);
            MessageBox.Show("Restore failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnPrimary.IsEnabled = true;
            BtnRestore.IsEnabled = true;
            SubtitleText.Text = "Restore finished";
        }
    }

    private void GoToCheck()
    {
        _lastEnv = _env.Check();
        var sb = new StringBuilder();
        sb.AppendLine($"OS: {_lastEnv.OsVersion} ({_lastEnv.Architecture})");
        sb.AppendLine($"Resolution: {_lastEnv.PrimaryScreenWidth}x{_lastEnv.PrimaryScreenHeight}");
        sb.AppendLine($"Rainmeter: {(_lastEnv.RainmeterInstalled ? "Installed" : "Not found")}");
        if (_lastEnv.RainmeterSkinsPath != null)
            sb.AppendLine($"Skins: {_lastEnv.RainmeterSkinsPath}");
        if (_lastEnv.Warnings.Count > 0)
        {
            sb.AppendLine(); sb.AppendLine("Warnings:");
            foreach (var w in _lastEnv.Warnings) sb.AppendLine("- " + w);
        }
        if (_lastEnv.BlockingIssues.Count > 0)
        {
            sb.AppendLine(); sb.AppendLine("Blocking:");
            foreach (var b in _lastEnv.BlockingIssues) sb.AppendLine("- " + b);
        }
        CheckResultText.Text = sb.ToString();
        ShowPanel(Step.Check);
        BtnPrimary.Content = _lastEnv.BlockingIssues.Count > 0 ? "Exit" : "Next";
        BtnSecondary.Content = "Recheck";
        SubtitleText.Text = "Environment check";
        BtnRestore.Visibility = Visibility.Collapsed;
        BtnFolderIcon.Visibility = Visibility.Collapsed;
        BtnNotes.Visibility = Visibility.Collapsed;
    }

    private void GoToOptions()
    {
        if (_lastEnv?.BlockingIssues.Count > 0) { Close(); return; }
        ShowPanel(Step.Options);
        BtnPrimary.Content = "Apply theme";
        BtnSecondary.Content = "Back";
        SubtitleText.Text = "Choose features";
        BtnRestore.Visibility = Visibility.Visible;
        BtnFolderIcon.Visibility = Visibility.Visible;
        BtnNotes.Visibility = Visibility.Collapsed;
    }

    private async Task RunApplyAsync()
    {
        ShowPanel(Step.Result);
        ResultTitle.Text = "Applying...";
        ResultBody.Text = "Backing up and configuring desktop.";
        BtnPrimary.IsEnabled = false;
        BtnRestore.IsEnabled = false;
        BtnSecondary.IsEnabled = false;
        BtnFolderIcon.IsEnabled = false;
        BtnNotes.IsEnabled = false;
        SubtitleText.Text = "Working";

        try
        {
            var themeRoot = ResolveThemeRoot();
            _lastThemeRoot = themeRoot;
            if (themeRoot == null)
            {
                ResultTitle.Text = "Theme resources not found";
                ResultBody.Text = "Place themes/Desktop-01 next to the app.";
                BtnPrimary.Content = "Close";
                BtnPrimary.IsEnabled = true;
                return;
            }

            var options = new ThemeApplyOptions
            {
                ApplyWallpaper = OptWallpaper.IsChecked == true,
                ApplyClock = OptClock.IsChecked == true,
                ApplyCalendar = OptCalendar.IsChecked == true,
                ApplyNotes = OptNotes.IsChecked == true
            };

            var result = await _apply.ApplyAsync(themeRoot, options).ConfigureAwait(true);

            if (result.Success)
            {
                ResultTitle.Text = "Done";
                var sb = new StringBuilder();
                sb.AppendLine(result.Message);
                if (!string.IsNullOrEmpty(result.BackupId))
                    sb.AppendLine("Backup: " + result.BackupId);
                foreach (var kv in result.Steps)
                    sb.AppendLine($"- {kv.Key}: {kv.Value}");
                foreach (var w in result.Warnings)
                    sb.AppendLine("! " + w);
                if (options.ApplyNotes)
                {
                    var launched = NotesLauncher.TryLaunch(_log);
                    sb.AppendLine(launched ? "- notes-ui: launched" : "- notes-ui: MilkBeanNotes.exe not found");
                }
                ResultBody.Text = sb.ToString();
            }
            else
            {
                ResultTitle.Text = "Apply incomplete";
                var sb = new StringBuilder();
                foreach (var err in result.Errors) sb.AppendLine("- " + err);
                foreach (var w in result.Warnings) sb.AppendLine("! " + w);
                sb.AppendLine(); sb.AppendLine("Log: " + _log.GetLatestLogPath());
                ResultBody.Text = sb.ToString();
            }
        }
        catch (Exception ex)
        {
            _log.Error("Apply exception", ex);
            ResultTitle.Text = "Error";
            ResultBody.Text = ex.Message + "\n\nLog: " + _log.GetLatestLogPath();
        }
        finally
        {
            BtnPrimary.Content = "Finish";
            BtnPrimary.IsEnabled = true;
            BtnRestore.IsEnabled = true;
            BtnSecondary.IsEnabled = true;
            BtnFolderIcon.IsEnabled = true;
            BtnNotes.IsEnabled = true;
            BtnSecondary.Content = "View log";
            BtnRestore.Visibility = Visibility.Visible;
            BtnFolderIcon.Visibility = Visibility.Visible;
            BtnNotes.Visibility = Visibility.Visible;
            SubtitleText.Text = "Set icons, open notes, or restore";
            _step = Step.Result;
        }
    }

    private void ShowPanel(Step step)
    {
        _step = step;
        PanelWelcome.Visibility = step == Step.Welcome ? Visibility.Visible : Visibility.Collapsed;
        PanelCheck.Visibility = step == Step.Check ? Visibility.Visible : Visibility.Collapsed;
        PanelOptions.Visibility = step == Step.Options ? Visibility.Visible : Visibility.Collapsed;
        PanelResult.Visibility = step == Step.Result ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string? ResolveThemeRoot()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "themes", "Desktop-01"),
            Path.Combine(baseDir, "..", "..", "..", "..", "themes", "Desktop-01"),
            Path.Combine(baseDir, "..", "..", "..", "themes", "Desktop-01")
        };
        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(Path.Combine(full, "theme.json"))) return full;
        }
        return null;
    }
}
