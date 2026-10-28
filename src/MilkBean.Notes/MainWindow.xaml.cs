using System.Windows;
using System.Windows.Input;

namespace MilkBean.Notes;

public partial class MainWindow : Window
{
    private List<NoteItem> _notes = new();
    private readonly Dictionary<string, NoteWindow> _openWindows = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _notes = await NoteStore.LoadAsync();
        foreach (var n in _notes)
        {
            if (string.IsNullOrWhiteSpace(n.Title))
                n.Title = string.IsNullOrWhiteSpace(n.Content) ? "空白便签" : n.Content.Split('\n')[0].Trim();
            if (n.Title.Length > 40) n.Title = n.Title[..40] + "\u2026";
        }
        NoteList.ItemsSource = null;
        NoteList.ItemsSource = _notes;
    }

    private async void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        var note = new NoteItem
        {
            Id = "note-" + Guid.NewGuid().ToString("N")[..8],
            Title = "新便签",
            Content = "",
            X = 120 + _notes.Count * 24,
            Y = 120 + _notes.Count * 24,
            Width = 280,
            Height = 220,
            UpdatedAt = DateTimeOffset.Now
        };
        _notes.Add(note);
        await NoteStore.SaveAsync(_notes);
        await ReloadAsync();
        OpenNote(note);
    }

    private void BtnOpenAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var n in _notes) OpenNote(n);
    }

    private void NoteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (NoteList.SelectedItem is NoteItem note) OpenNote(note);
    }

    private void OpenNote(NoteItem note)
    {
        if (_openWindows.TryGetValue(note.Id, out var existing) && existing.IsLoaded)
        {
            existing.Activate();
            return;
        }
        var win = new NoteWindow(note, OnNoteSaved, OnNoteClosed);
        _openWindows[note.Id] = win;
        win.Show();
    }

    private async void OnNoteSaved(NoteItem note)
    {
        var idx = _notes.FindIndex(n => n.Id == note.Id);
        if (idx >= 0) _notes[idx] = note; else _notes.Add(note);
        await NoteStore.SaveAsync(_notes);
        await ReloadAsync();
    }

    private void OnNoteClosed(string noteId) => _openWindows.Remove(noteId);
}
