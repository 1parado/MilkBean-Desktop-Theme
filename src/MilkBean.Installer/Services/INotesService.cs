namespace MilkBean.Installer.Services;

public interface INotesService
{
    Task<IReadOnlyList<NoteItem>> LoadAsync();
    Task SaveAsync(IEnumerable<NoteItem> notes);
    Task EnsureStoreExistsAsync();
    string GetStorePath();
}

public sealed class NoteItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 280;
    public double Height { get; set; } = 220;
    public DateTimeOffset UpdatedAt { get; set; }
}
