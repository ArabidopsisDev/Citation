using Citation.Model.Reference;

namespace Citation.Model;

public class TimelineItem
{
    public JournalArticle Article { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public System.Windows.Media.Brush Color { get; set; }
    public string Conclusion { get; set; }
    public string Title { get; set; }
    public string Folder { get; set; }
}
