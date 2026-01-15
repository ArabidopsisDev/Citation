using Citation.Model;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Citation.View.Page;

public partial class AddNotePage : UserControl
{
    Note? prevNote;

    public AddNotePage(Note? note = null)
    {
        InitializeComponent();

        prevNote = note;
        if (note is not null)
        {
            LoadFromRtf(NoteEditor, note.RichText);
            NoteTitleBox.Text = note.Title;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = NoteTitleBox.Text.Trim();
        var mainWindow = Application.Current.MainWindow as MainWindow;

        if (string.IsNullOrEmpty(title))
        {
            mainWindow!.ShowToast(Properties.Resources.AddNotePage_LackName);
            return;
        }

        var rtf = GetRtfFromRichTextBox(NoteEditor);
        var now = DateTime.Now;
        var note = new Note(title, rtf, now);

        if (prevNote is not null)
            prevNote.DeleteSql(Acceed.Shared.Connection);
        note.ToSql(Acceed.Shared.Connection);
        prevNote = note;
        mainWindow!.ShowToast(Properties.Resources.AddNotePage_Success);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        NoteTitleBox.Text = string.Empty;
        NoteEditor.Document.Blocks.Clear();
    }

    public static string GetRtfFromRichTextBox(RichTextBox rtb)
    {
        TextRange range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        using (MemoryStream ms = new MemoryStream())
        {
            range.Save(ms, DataFormats.Rtf);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }
    }

    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        ColorPopup.IsOpen = !ColorPopup.IsOpen;
    }

    private void XceedColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (e.NewValue is not null)
        {
            NoteEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(e.NewValue.Value));
            ColorPopup.IsOpen = false;
        }
    }

    private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var combo = sender as ComboBox;
        if (combo != null && combo.SelectedItem is ComboBoxItem item)
        {
            var family = item.FontFamily;
            NoteEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, family);
        }
    }

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var combo = sender as ComboBox;
        if (combo != null && combo.SelectedItem is ComboBoxItem item)
        {
            if (double.TryParse(item.Content.ToString(), out var size))
            {
                NoteEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
            }
        }
    }

    public void LoadFromRtf(RichTextBox richTextBox, string rtfContent)
    {
        if (string.IsNullOrEmpty(rtfContent)) return;

        using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(rtfContent)))
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            textRange.Load(stream, DataFormats.Rtf);
        }
    }
}