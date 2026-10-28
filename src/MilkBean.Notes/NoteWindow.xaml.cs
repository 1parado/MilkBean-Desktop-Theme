using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MilkBean.Notes;

public partial class NoteWindow : Window
{
    private readonly NoteItem _note;
    private readonly Action<NoteItem> _onSaved;
    private readonly Action<string> _onClosed;
    private readonly DispatcherTimer _saveTimer;
    private bool _dirty;

    public NoteWindow(NoteItem note, Action<NoteItem> onSaved, Action<string> onClosed)
    {
        InitializeComponent();
        _note = note;
        _onSaved = onSaved;
        _onClosed = onClosed;

        TitleBox.Text = note.Title;
        BodyBox.Text = note.Content;
        Left = note.X;
        Top = note.Y;
        Width = note.Width > 120 ? note.Width : 280;
        Height = note.Height > 100 ? note.Height : 220;

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); if (_dirty) Persist(); };

        Closing += (_, _) => { Persist(); _onClosed(_note.Id); };
        SizeChanged += (_, _) => { _note.Width = Width; _note.Height = Height; MarkDirty(); };
        LocationChanged += (_, _) => { _note.X = Left; _note.Y = Top; MarkDirty(); };
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void Content_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e) => MarkDirty();

    private void MarkDirty()
    {
        _dirty = true;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void Persist()
    {
        _note.Title = TitleBox.Text?.Trim() ?? "";
        _note.Content = BodyBox.Text ?? "";
        _note.X = Left;
        _note.Y = Top;
        _note.Width = Width;
        _note.Height = Height;
        _note.UpdatedAt = DateTimeOffset.Now;
        if (string.IsNullOrWhiteSpace(_note.Title))
            _note.Title = string.IsNullOrWhiteSpace(_note.Content) ? "空白便签" : _note.Content.Split('\n')[0].Trim();
        _dirty = false;
        _onSaved(_note);
    }
}
