using System.IO;
using System.Text.Json;

namespace MilkBean.Notes;

/// <summary>
/// Shared notes file: %LOCALAPPDATA%\MilkBean\Config\notes.json
/// </summary>
public static class NoteStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string Root =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MilkBean");

    public static string ConfigDir => Path.Combine(Root, "Config");
    public static string NotesPath => Path.Combine(ConfigDir, "notes.json");

    public static async Task EnsureAsync()
    {
        Directory.CreateDirectory(ConfigDir);
        if (!File.Exists(NotesPath))
            await WriteAtomicAsync(new NotesDocument()).ConfigureAwait(false);
    }

    public static async Task<List<NoteItem>> LoadAsync()
    {
        await EnsureAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(NotesPath).ConfigureAwait(false);
            var doc = JsonSerializer.Deserialize<NotesDocument>(json, JsonOptions);
            return doc?.Notes ?? new List<NoteItem>();
        }
        catch { return new List<NoteItem>(); }
    }

    public static async Task SaveAsync(IEnumerable<NoteItem> notes)
    {
        Directory.CreateDirectory(ConfigDir);
        await WriteAtomicAsync(new NotesDocument { SchemaVersion = 1, Notes = notes.ToList() }).ConfigureAwait(false);
    }

    private static async Task WriteAtomicAsync(NotesDocument doc)
    {
        var tmp = NotesPath + ".tmp";
        await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(doc, JsonOptions)).ConfigureAwait(false);
        File.Move(tmp, NotesPath, overwrite: true);
    }
}

public sealed class NotesDocument
{
    public int SchemaVersion { get; set; } = 1;
    public List<NoteItem> Notes { get; set; } = new();
}

public sealed class NoteItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
    public double Width { get; set; } = 280;
    public double Height { get; set; } = 220;
    public DateTimeOffset UpdatedAt { get; set; }
}
