using System.Text.Json;

namespace MilkBean.Installer.Services;

public sealed class ThemeManifestService : IThemeManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly ILoggingService _log;

    public ThemeManifestService(ILoggingService log) => _log = log;

    public ThemeManifest Load(string themeRoot)
    {
        var path = Path.Combine(themeRoot, "theme.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("theme.json not found", path);

        var json = File.ReadAllText(path);
        var manifest = JsonSerializer.Deserialize<ThemeManifest>(json, JsonOptions)
                       ?? throw new InvalidDataException("Failed to parse theme.json");

        _log.Info($"Loaded theme: {manifest.ThemeId} v{manifest.Version}");
        return manifest;
    }

    public ValidationResult Validate(ThemeManifest manifest, string themeRoot)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(manifest.ThemeId))
            result.Errors.Add("themeId is empty");

        if (manifest.Wallpaper != null)
        {
            var rel = manifest.Wallpaper.Default;
            if (string.IsNullOrWhiteSpace(rel))
                result.Errors.Add("wallpaper.default not set");
            else
            {
                var full = NormalizeUnderRoot(themeRoot, rel);
                if (full == null)
                    result.Errors.Add($"Invalid wallpaper path: {rel}");
                else if (!File.Exists(full))
                    result.Errors.Add($"Wallpaper file missing: {rel}");
            }
        }
        else
        {
            result.Warnings.Add("No wallpaper configured");
        }

        foreach (var icon in manifest.Icons)
        {
            var full = NormalizeUnderRoot(themeRoot, icon.Path);
            if (full == null)
                result.Errors.Add($"Invalid icon path: {icon.Path}");
            else if (!File.Exists(full))
                result.Warnings.Add($"Icon file not provided yet: {icon.Path}");
        }

        _log.Info($"Validate: valid={result.IsValid}, errors={result.Errors.Count}, warnings={result.Warnings.Count}");
        return result;
    }

    public static string? NormalizeUnderRoot(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) return null;
        var combined = Path.GetFullPath(Path.Combine(root, relative));
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(combined.TrimEnd(Path.DirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            return null;
        return combined;
    }
}
