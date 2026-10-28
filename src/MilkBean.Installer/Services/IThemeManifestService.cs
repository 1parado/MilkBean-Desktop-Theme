namespace MilkBean.Installer.Services;

public interface IThemeManifestService
{
    ThemeManifest Load(string themeRoot);
    ValidationResult Validate(ThemeManifest manifest, string themeRoot);
}

public sealed class ThemeManifest
{
    public int SchemaVersion { get; set; }
    public string ThemeId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public WallpaperConfig? Wallpaper { get; set; }
    public List<WidgetConfig> Widgets { get; set; } = new();
    public List<IconConfig> Icons { get; set; } = new();
    public Dictionary<string, string>? Assets { get; set; }
    public Dictionary<string, string>? Colors { get; set; }
}

public sealed class WallpaperConfig
{
    public string Default { get; set; } = "";
    public List<string>? Variants { get; set; }
    public string FitMode { get; set; } = "fill";
}

public sealed class WidgetConfig
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Skin { get; set; }
    public bool EnabledByDefault { get; set; } = true;
}

public sealed class IconConfig
{
    public string Id { get; set; } = "";
    public string Path { get; set; } = "";
    public string? Label { get; set; }
}

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
