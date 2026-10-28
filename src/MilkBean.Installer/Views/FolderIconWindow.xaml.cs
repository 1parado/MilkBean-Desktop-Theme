using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using MilkBean.Installer.Services;
using MessageBox = System.Windows.MessageBox;

namespace MilkBean.Installer.Views;

public partial class FolderIconWindow : Window
{
    private readonly IFolderIconService _icons;
    private readonly ILoggingService _log;
    private readonly string _themeRoot;
    private List<IconOption> _options = new();

    public FolderIconWindow(IFolderIconService icons, ILoggingService log, string themeRoot)
    {
        InitializeComponent();
        _icons = icons;
        _log = log;
        _themeRoot = themeRoot;
        Loaded += (_, _) => LoadIcons();
    }

    private void LoadIcons()
    {
        _options.Clear();
        var iconsDir = Path.Combine(_themeRoot, "Icons");
        if (Directory.Exists(iconsDir))
        {
            foreach (var file in Directory.GetFiles(iconsDir, "*.ico"))
            {
                _options.Add(new IconOption
                {
                    Label = Path.GetFileNameWithoutExtension(file),
                    Path = file
                });
            }
        }
        if (_options.Count == 0)
            StatusText.Text = "未找到 Icons 目录下的 .ico 文件。请先将奶豆图标放入 themes/Desktop-01/Icons/";
        IconList.ItemsSource = _options;
        if (_options.Count > 0) IconList.SelectedIndex = 0;
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "选择要设置图标的文件夹",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            FolderPathBox.Text = dlg.SelectedPath;
    }

    private async void BtnApply_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPathBox.Text?.Trim();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show("请先选择有效的文件夹。", "提示");
            return;
        }
        if (IconList.SelectedItem is not IconOption icon || !File.Exists(icon.Path))
        {
            MessageBox.Show("请选择可用的图标文件。", "提示");
            return;
        }
        StatusText.Text = "正在应用\u2026";
        try
        {
            var backup = await _icons.ApplyIconAsync(folder, icon.Path);
            if (backup.Success)
            {
                await AppendFolderIconBackupAsync(backup);
                StatusText.Text = "应用成功。恢复原状时会尝试还原此文件夹图标。";
                MessageBox.Show("文件夹图标已更新。", "成功");
            }
            else
            {
                StatusText.Text = "应用失败，请查看日志或权限。";
                MessageBox.Show("应用失败，可能没有写入权限。", "失败");
            }
        }
        catch (Exception ex)
        {
            _log.Error("Folder icon apply UI error", ex);
            StatusText.Text = ex.Message;
            MessageBox.Show(ex.Message, "错误");
        }
    }

    private async Task AppendFolderIconBackupAsync(FolderIconBackup backup)
    {
        try
        {
            AppPaths.EnsureDirectories();
            var path = Path.Combine(AppPaths.ConfigDir, "folder-icons-applied.json");
            var list = new List<FolderIconBackup>();
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                list = JsonSerializer.Deserialize<List<FolderIconBackup>>(json) ?? new();
            }
            list.RemoveAll(x => string.Equals(x.FolderPath, backup.FolderPath, StringComparison.OrdinalIgnoreCase));
            list.Add(backup);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            _log.Warn("Could not persist folder icon backup: " + ex.Message);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

    private sealed class IconOption
    {
        public string Label { get; set; } = "";
        public string Path { get; set; } = "";
    }
}
