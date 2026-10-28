using System.Text.Json;

namespace MilkBean.Installer.Services;

public sealed class NotesService : INotesService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILoggingService _log;

    public NotesService(ILoggingService log) => _log = log;

    public string GetStorePath() => AppPaths.NotesPath;

    public async Task EnsureStoreExistsAsync()
    {
        AppPaths.EnsureDirectories();
        if (!File.Exists(AppPaths.NotesPath))
        {
            var empty = new NotesDocument { SchemaVersion = 1, Notes = new List<NoteItem>() };
            await WriteAtomicAsync(empty).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<NoteItem>> LoadAsync()
    {
        await EnsureStoreExistsAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(AppPaths.NotesPath).ConfigureAwait(false);
            var doc = JsonSerializer.Deserialize<NotesDocument>(json, JsonOptions);
            return doc?.Notes ?? new List<NoteItem>();
        }
        catch (Exception ex)
        {
            _log.Error("Failed to load notes", ex);
            return Array.Empty<NoteItem>();
        }
    }

    public async Task SaveAsync(IEnumerable<NoteItem> notes)
    {
        AppPaths.EnsureDirectories();
        var doc = new NotesDocument { SchemaVersion = 1, Notes = notes.ToList() };
        await WriteAtomicAsync(doc).ConfigureAwait(false);
        _log.Info($"Saved {doc.Notes.Count} notes");
    }

    private async Task WriteAtomicAsync(NotesDocument doc)
    {
        var path = AppPaths.NotesPath;
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(doc, JsonOptions);
        await File.WriteAllTextAsync(tmp, json).ConfigureAwait(false);
        File.Move(tmp, path, overwrite: true);
    }

    private sealed class NotesDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public List<NoteItem> Notes { get; set; } = new();
    }
}
